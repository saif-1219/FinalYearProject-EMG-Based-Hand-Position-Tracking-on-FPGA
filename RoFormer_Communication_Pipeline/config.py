"""
Configuration file for the RoFormer sEMG Pipeline.

Centralize all settings here to make the project portable and configurable.
Users can modify these values or set environment variables to override defaults.
"""

import os
from pathlib import Path

# ══════════════════════════════════════════════════════════════
#  PROJECT PATHS
# ══════════════════════════════════════════════════════════════

PROJECT_DIR = os.path.dirname(os.path.abspath(__file__))

# Model checkpoint - download from GitHub releases and place here
CHECKPOINT = os.getenv(
    "ROFORMER_CHECKPOINT",
    os.path.join(PROJECT_DIR, "roformer.pt")
)

# Input data path - users should provide their own MAT file
# Example: python gui.py --mat-path "path/to/data.mat"
DEFAULT_MAT_PATH = os.getenv(
    "ROFORMER_MAT_PATH",
    None  # User must provide this
)

# ══════════════════════════════════════════════════════════════
#  EMG PROCESSING PARAMETERS
# ══════════════════════════════════════════════════════════════

WINDOW_SIZE = 400          # EMG window length (samples @ 2kHz = 200ms)
STRIDE = 50                # Sliding window stride
SAMPLING_RATE = 2000       # Hz

# Bandpass filter settings
BANDPASS_LOWCUT = 5        # Hz
BANDPASS_HIGHCUT = 500     # Hz
BANDPASS_ORDER = 4

# ──────────────────────────────────────────────────────────────
# Kalman Filter Settings
# ──────────────────────────────────────────────────────────────

KALMAN_Q = 1e-4            # Process noise (higher = faster response)
KALMAN_R = 0.05            # Measurement noise (higher = smoother line)
KALMAN_USE_ADAPTIVE = True # Use adaptive Kalman filter

# ══════════════════════════════════════════════════════════════
#  NETWORK CONFIGURATION
# ══════════════════════════════════════════════════════════════

# ── Local Model Server ───────────────────────────────────────
LOCAL_HOST = os.getenv("ROFORMER_LOCAL_HOST", "127.0.0.1")
LOCAL_PORT = int(os.getenv("ROFORMER_LOCAL_PORT", "5005"))
LOCAL_PROTOCOL = os.getenv("ROFORMER_LOCAL_PROTOCOL", "udp")  # "udp" or "uart"

# ── FPGA Configuration ───────────────────────────────────────
FPGA_IP = os.getenv("ROFORMER_FPGA_IP", "192.168.1.10")
FPGA_PORT = int(os.getenv("ROFORMER_FPGA_PORT", "5005"))
USE_FPGA = os.getenv("ROFORMER_USE_FPGA", "false").lower() == "true"

# ── Serial/UART Configuration ────────────────────────────────
SERIAL_PORT = os.getenv("ROFORMER_SERIAL_PORT", "COM4")
SERIAL_BAUDRATE = int(os.getenv("ROFORMER_SERIAL_BAUDRATE", "115200"))

# ── Timeouts ─────────────────────────────────────────────────
UDP_TIMEOUT = 5.0           # seconds
SERIAL_TIMEOUT = 5.0        # seconds

# ══════════════════════════════════════════════════════════════
#  DEVICE CONFIGURATION
# ══════════════════════════════════════════════════════════════

# Auto-detect GPU, fall back to CPU
try:
    import torch
    DEVICE = "cuda" if torch.cuda.is_available() else "cpu"
except ImportError:
    DEVICE = "cpu"

DEVICE = os.getenv("ROFORMER_DEVICE", DEVICE)

# ══════════════════════════════════════════════════════════════
#  JOINT NAMES (10 DOF hand model)
# ══════════════════════════════════════════════════════════════

JOINT_NAMES = [
    "Thumb MCP",
    "Thumb PIP",
    "Index MCP",
    "Index PIP",
    "Middle MCP",
    "Middle PIP",
    "Ring MCP",
    "Ring PIP",
    "Pinky MCP",
    "Pinky PIP",
]

# ══════════════════════════════════════════════════════════════
#  HELPER FUNCTION TO CHECK CONFIG
# ══════════════════════════════════════════════════════════════

def validate_config():
    """Validate that required files exist."""
    if not os.path.exists(CHECKPOINT):
        raise FileNotFoundError(
            f"Model checkpoint not found at {CHECKPOINT}. "
            f"Please download roformer.pt from GitHub releases."
        )
    return True
