import os
import numpy as np
import glob
import scipy.io as scio
from scipy.signal import butter, sosfiltfilt

# Glove columns corresponding to 10 key finger joints
GLOVE_COLS = [1, 2, 4, 5, 7, 8, 11, 12, 15, 16]

def mu_law_normalize(x, mu=255):
    """
    Apply μ-law companding to EMG data.
    Input x should be normalized to [-1,1].
    """
    return np.sign(x) * np.log1p(mu * np.abs(x)) / np.log1p(mu)

def bandpass_butterworth(emg, lowcut=5, highcut=500, fs=2000, order=4):
    """
    Apply a Butterworth bandpass filter to EMG data.
    
    Args:
    - emg: [time, channels] array
    - lowcut: Low frequency cutoff (Hz)
    - highcut: High frequency cutoff (Hz)
    - fs: Sampling rate (Hz)
    - order: Filter order
    
    Returns:
    - Filtered emg array
    """
    nyq = 0.5 * fs
    low = lowcut / nyq
    high = highcut / nyq
    sos = butter(order, [low, high], btype='band', output='sos')
    return sosfiltfilt(sos, emg, axis=0)


# ──────────────────────────────────────────────────────────────
# Single-subject .mat loader  (used by pipeline)
# ──────────────────────────────────────────────────────────────
def load_single_subject_mat(mat_path):
    """
    Load a single NinaPro .mat file and return raw arrays.

    Returns:
        emg   : np.float32  [time, 12]  – raw sEMG
        glove : np.float32  [time, 22]  – raw glove angles
        restimulus : np.int32 [time]    – gesture labels
    """
    data = scio.loadmat(mat_path)
    emg   = data['emg'].astype(np.float32)
    glove = data['glove'].astype(np.float32)
    restimulus = data['restimulus'].astype(np.int32).squeeze()
    return emg, glove, restimulus


def preprocess_emg(emg, mu=255, lowcut=5, highcut=500, fs=2000, order=4):
    """
    Full EMG preprocessing: bandpass → rectify → normalize → μ-law.
    Input:  [time, channels]
    Output: [time, channels]
    """
    emg = bandpass_butterworth(emg, lowcut, highcut, fs, order)
    emg = np.abs(emg)
    emg = emg / (np.max(np.abs(emg), axis=0, keepdims=True) + 1e-8)
    emg = mu_law_normalize(emg, mu=mu)
    return emg


def preprocess_glove(glove, cols=None):
    """
    Glove min-max normalization and column selection.
    Returns: normalized glove [time, len(cols)], glove_min, glove_max
    """
    if cols is None:
        cols = GLOVE_COLS
    glove_min = np.min(glove, axis=0, keepdims=True)
    glove_max = np.max(glove, axis=0, keepdims=True)
    glove = (glove - glove_min) / (glove_max - glove_min + 1e-8)
    return glove[:, cols], glove_min[:, cols], glove_max[:, cols]


def segment_windows(emg, glove, restimulus, window_size=400, stride=50):
    """
    Sliding-window segmentation.
    Returns: emg_windows [N, window_size, C], glove_windows [N, J], glove_diff_windows [N, J], restim_windows [N]
    """
    emg_wins, glove_wins, glove_diff_wins, restim_wins = [], [], [], []
    for i in range(0, len(emg) - window_size, stride):
        emg_wins.append(emg[i:i+window_size, :])
        
        # Target position (end of window)
        glove_last = glove[i+window_size-1, :]
        # Start position
        glove_first = glove[i, :]
        
        glove_wins.append(glove_last)
        glove_diff_wins.append(glove_last - glove_first)
        restim_wins.append(restimulus[i+window_size-1])
        
    return np.array(emg_wins), np.array(glove_wins), np.array(glove_diff_wins), np.array(restim_wins)


# ──────────────────────────────────────────────────────────────
# Legacy multi-subject batch processor  (kept for future use)
# ──────────────────────────────────────────────────────────────
def process_dataset_for_oneSubject(subject, base_path, window_size=400, stride=50, mu=255):
    """Process and save windowed data for one subject directory."""
    print(f"Processing subject: {subject}")
    data_paths = sorted(glob.glob(os.path.join(base_path, subject, subject, '*.mat')))

    emg_all, glove_all, restim_all = [], [], []
    for path in data_paths[:2]:  # use first two sessions
        data = scio.loadmat(path)
        emg_all.append(data['emg'])
        glove_all.append(data['glove'])
        restim_all.append(data['restimulus'])

    emg   = np.vstack(emg_all).astype(np.float32)
    glove = np.vstack(glove_all).astype(np.float32)
    restimulus = np.vstack(restim_all).astype(np.int32).squeeze()

    emg = preprocess_emg(emg, mu=mu)
    glove, glove_min, glove_max = preprocess_glove(glove)
    emg_windows, glove_windows, glove_diff_windows, restim_windows = segment_windows(
        emg, glove, restimulus, window_size, stride
    )

    output_dir = os.path.join(base_path, f"processed/{subject}")
    os.makedirs(output_dir, exist_ok=True)
    np.savez(os.path.join(output_dir, "data.npz"),
             sEMG=emg_windows, angles=glove_windows, angle_diff=glove_diff_windows, restim=restim_windows)
    print(f"Saved {emg_windows.shape[0]} samples for {subject}")



# ──────────────────────────────────────────────────────────────
# PyTorch Dataset  (requires torch — only imported when used)
# ──────────────────────────────────────────────────────────────
try:
    import torch
    from torch.utils.data import Dataset

    class NinaproDataset(Dataset):
        def __init__(self, npz_files, split='train', split_ratio=0.8):
            sEMG_all, angles_all, angle_diff_all = [], [], []

            for f in npz_files:
                data = np.load(f, mmap_mode='r')
                sEMG = data['sEMG']
                angles = data['angles']
                angle_diff = data['angle_diff']
                restim = data['restim']

                n_movement_1 = np.sum(restim == 1)

                unique_reps = np.unique(restim)
                for rep in unique_reps:
                    idx = np.where(restim == rep)[0]

                    # Balancing: downsample rest class
                    if rep == 0 and len(idx) > n_movement_1:
                        np.random.shuffle(idx)
                        idx = idx[:n_movement_1]

                    idx.sort()

                    split_idx = int(split_ratio * len(idx))
                    if split == 'train':
                        selected_idx = idx[:split_idx]
                    else:
                        selected_idx = idx[split_idx:]

                    if len(selected_idx) > 0:
                        sEMG_all.append(sEMG[selected_idx])
                        angles_all.append(angles[selected_idx])
                        angle_diff_all.append(angle_diff[selected_idx])

            self.sEMG = np.concatenate(sEMG_all, axis=0)
            self.angles = np.concatenate(angles_all, axis=0)
            self.angle_diff = np.concatenate(angle_diff_all, axis=0)

        def __len__(self):
            return len(self.sEMG)

        def __getitem__(self, idx):
            sEMG = torch.tensor(self.sEMG[idx].T, dtype=torch.float32)
            current_angle = torch.tensor(self.angles[idx], dtype=torch.float32)
            diff = torch.tensor(self.angle_diff[idx], dtype=torch.float32)
            return sEMG, current_angle, diff

except ImportError:
    pass  # torch not available — dataset class won't be used