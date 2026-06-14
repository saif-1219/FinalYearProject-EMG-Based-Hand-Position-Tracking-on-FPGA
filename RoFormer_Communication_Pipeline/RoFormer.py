import os
import random
import torch
import torch.nn as nn
import torch.optim as optim


# ──────────────────────────────────────────────────────────────
# Config
# ──────────────────────────────────────────────────────────────
class Config:
    exp_name = 'roformer_sEMG'
    data_path = '/content/processed_ninapro_db2.npz'  # Processed NPZ file (sEMG and labels)
    save_dir = 'outputs'
    max_seq_len = 400  # Time steps (50ms at 2kHz)
    num_channels = 12  # sEMG channels
    num_angles = 10  # Output joint angles
    batch_size = 32
    lr = 1e-4
    epochs = 50
    seed = 42
    num_heads = 8
    num_layers = 2
    d_model = 128
    d_ff = 256
    dropout = 0.1
    patience = 6


# ──────────────────────────────────────────────────────────────
# RoPE (Rotary Position Embedding)
# ──────────────────────────────────────────────────────────────
class RotaryEmbedding(nn.Module):
    def __init__(self, dim, max_position_embeddings=2048, base=10000, device=None):
        super().__init__()
        self.dim = dim
        self.max_position_embeddings = max_position_embeddings
        self.base = base
        inv_freq = 1.0 / (self.base ** (torch.arange(0, self.dim, 2).float().to(device) / self.dim))
        self.register_buffer("inv_freq", inv_freq)

        # Build here to make `torch.jit.trace` work
        self._set_cos_sin_cache(
            seq_len=max_position_embeddings, device=self.inv_freq.device, dtype=torch.get_default_dtype()
        )

    def _set_cos_sin_cache(self, seq_len, device, dtype):
        self.max_seq_len_cached = seq_len
        t = torch.arange(self.max_seq_len_cached, device=device, dtype=self.inv_freq.dtype)

        freqs = torch.einsum("i,j->ij", t, self.inv_freq)
        # Different from paper, but it uses a different permutation in order to obtain the same calculation
        emb = torch.cat((freqs, freqs), dim=-1)
        self.register_buffer("cos_cached", emb.cos().to(dtype), persistent=False)
        self.register_buffer("sin_cached", emb.sin().to(dtype), persistent=False)

    def forward(self, x, seq_len=None):
        # x: [bs, num_attention_heads, seq_len, head_size]
        if seq_len > self.max_seq_len_cached:
            self._set_cos_sin_cache(seq_len=seq_len, device=x.device, dtype=x.dtype)
        return (
            self.cos_cached[:seq_len].to(dtype=x.dtype),
            self.sin_cached[:seq_len].to(dtype=x.dtype),
        )


def rotate_half(x):
    """Rotates half the hidden dims of the input."""
    x1 = x[..., : x.shape[-1] // 2]
    x2 = x[..., x.shape[-1] // 2 :]
    return torch.cat((-x2, x1), dim=-1)

def apply_rotary_pos_emb(q, k, cos, sin):
    # q, k: [batch, seq_len, d_model]
    # cos, sin: [seq_len, d_model]
    cos = cos.unsqueeze(0)  # [1, seq_len, d_model]
    sin = sin.unsqueeze(0)
    q_embed = (q * cos) + (rotate_half(q) * sin)
    k_embed = (k * cos) + (rotate_half(k) * sin)
    return q_embed, k_embed


# ──────────────────────────────────────────────────────────────
# Transformer Layer
# ──────────────────────────────────────────────────────────────
class RoFormerLayer(nn.Module):
    def __init__(self, d_model, nhead, dim_feedforward=512, dropout=0.1):
        super().__init__()
        self.self_attn = nn.MultiheadAttention(d_model, nhead, dropout=dropout, batch_first=True)
        self.linear1 = nn.Linear(d_model, dim_feedforward)
        self.dropout = nn.Dropout(dropout)
        self.linear2 = nn.Linear(dim_feedforward, d_model)
        self.norm1 = nn.LayerNorm(d_model)
        self.norm2 = nn.LayerNorm(d_model)
        self.dropout1 = nn.Dropout(dropout)
        self.dropout2 = nn.Dropout(dropout)
        # self.activation = nn.GELU()
        self.activation = nn.ReLU()
    def forward(self, src, cos=None, sin=None, src_mask=None, src_key_padding_mask=None):
        q = k = src
        if cos is not None and sin is not None:
            q, k = apply_rotary_pos_emb(q, k, cos, sin)  # no unsqueeze
        src2 = self.self_attn(q, k, src, attn_mask=src_mask, key_padding_mask=src_key_padding_mask)[0]
        src = src + self.dropout1(src2)
        src = self.norm1(src)
        src2 = self.linear2(self.dropout(self.activation(self.linear1(src))))
        src = src + self.dropout2(src2)
        src = self.norm2(src)
        return src


# ──────────────────────────────────────────────────────────────
# Velocity-Guided Loss  (used during training)
# ──────────────────────────────────────────────────────────────
class PositionGuidedVelocityLoss(nn.Module):
    def __init__(self, lambda_p=0.1):
        super().__init__()
        self.mse = nn.MSELoss()
        self.lambda_p = lambda_p

    def forward(self, pred_diff, true_diff, prev_angle, true_curr_angle):
        # 1. Primary Loss: Velocity (Standard MSE on the model's direct output)
        loss_vel = self.mse(pred_diff, true_diff)

        # 2. Position Regularization
        # Reconstruct the predicted absolute position: Previous Angle + Predicted Change
        pred_curr_angle = prev_angle + pred_diff
        
        # Calculate how far off the reconstructed position is from the true position
        loss_pos = self.mse(pred_curr_angle, true_curr_angle)
        
        # Return combined loss (Velocity is the main driver, Position prevents drift)
        return loss_vel + (self.lambda_p * loss_pos)
class VelocityGuidedLoss(nn.Module):
    def __init__(self, lambda_v=0.1):
        super().__init__()
        self.mse = nn.MSELoss()
        self.lambda_v = lambda_v

    def forward(self, y_pred, y_curr, y_prev):
        # 1. Position Loss (Standard MSE)
        loss_mse = self.mse(y_pred, y_curr)

        # 2. Velocity Regularization
        # True Velocity: Where the hand ACTUALLY moved (Current - Prev)
        true_velocity = y_curr - y_prev
        
        # Predicted Velocity: Where the model moved relative to the starting point
        # (Predicted_Current - Ground_Truth_Prev)
        pred_velocity = y_pred - y_prev
        
        loss_vel = self.mse(pred_velocity, true_velocity)
        
        return loss_mse + (self.lambda_v * loss_vel)


# ──────────────────────────────────────────────────────────────
# RoFormer Model
# ──────────────────────────────────────────────────────────────
class RoFormer(nn.Module):
    def __init__(self, num_channels=12, time_steps=100, num_layers=4, num_heads=8, d_model=128, d_ff=512, num_angles=10, dropout=0.1):
        super().__init__()
        self.conv1d = nn.Conv1d(num_channels, d_model, kernel_size=3, padding=1)  # Conv1D on sEMG channels
        self.rope = RotaryEmbedding(d_model)
        self.layers = nn.ModuleList([RoFormerLayer(d_model, num_heads, d_ff, dropout) for _ in range(num_layers)])
        self.fc = nn.Linear(d_model, num_angles)
        self.dropout = nn.Dropout(dropout)

    def forward(self, x):
        # x: [batch, channels, time_steps]
        x = self.conv1d(x)                # [batch, d_model, time_steps]
        x = x.transpose(1, 2)             # [batch, time_steps, d_model]
        x = self.dropout(x)
    
        seq_len = x.shape[1]
        cos, sin = self.rope(x, seq_len=seq_len)
    
        for layer in self.layers:
            x = layer(x, cos, sin)
    
        x = x.mean(dim=1)
        x = self.fc(x)
        return x


# ──────────────────────────────────────────────────────────────
# Helper: load pretrained checkpoint
# ──────────────────────────────────────────────────────────────
def load_roformer(checkpoint_path, cfg=None, device=None):
    """
    Instantiate a RoFormer and load weights from a checkpoint.
    Handles DataParallel-saved weights (module.* prefix stripping).
    """
    if cfg is None:
        cfg = Config()
    if device is None:
        device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')

    model = RoFormer(
        num_channels=cfg.num_channels,
        time_steps=cfg.max_seq_len,
        num_layers=cfg.num_layers,
        num_heads=cfg.num_heads,
        d_model=cfg.d_model,
        d_ff=cfg.d_ff,
        num_angles=cfg.num_angles,
        dropout=cfg.dropout,
    )

    state_dict = torch.load(checkpoint_path, map_location=device, weights_only=True)

    # Strip 'module.' prefix from DataParallel-saved checkpoints
    cleaned = {}
    for k, v in state_dict.items():
        new_key = k.replace("module.", "", 1) if k.startswith("module.") else k
        cleaned[new_key] = v

    model.load_state_dict(cleaned, strict=False)
    model.to(device)
    model.eval()
    return model
