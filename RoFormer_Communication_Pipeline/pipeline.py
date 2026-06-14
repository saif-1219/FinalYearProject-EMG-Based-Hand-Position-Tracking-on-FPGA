"""
End-to-end sEMG -> Joint Angle pipeline.

ALL inference goes through UDP (local model server or FPGA).
Toggle USE_FPGA to choose the target.
"""

import os
import sys
import time
import socket
import numpy as np
import matplotlib.pyplot as plt

from load_dataset import (
    load_single_subject_mat,
    preprocess_emg,
    preprocess_glove,
    segment_windows,
)
from RoFormer import Config
from Kalman import apply_kalman_smoothing
import config

# ══════════════════════════════════════════════════════════════
#  CONFIGURATION - Use centralized config.py
# ══════════════════════════════════════════════════════════════

MAT_PATH = config.DEFAULT_MAT_PATH or input("Enter path to EMG MAT file: ")
WINDOW_SIZE = config.WINDOW_SIZE
STRIDE = config.STRIDE

# ── Processing path ──────────────────────────────────────────
USE_FPGA = config.USE_FPGA

# ── UDP (used for BOTH local and FPGA) ───────────────────────
LOCAL_HOST = config.LOCAL_HOST
LOCAL_PORT = config.LOCAL_PORT
FPGA_IP = config.FPGA_IP
FPGA_PORT = config.FPGA_PORT
UDP_TIMEOUT = config.UDP_TIMEOUT

# ── Kalman ───────────────────────────────────────────────────
KALMAN_Q = config.KALMAN_Q
KALMAN_R = config.KALMAN_R
MAX_WINDOWS = 500

JOINT_NAMES = config.JOINT_NAMES

# (Removed old hardcoded paths)


# ══════════════════════════════════════════════════════════════
#  UDP INFERENCE  (unified for local + FPGA)
# ══════════════════════════════════════════════════════════════
def udp_inference(emg_windows, server_addr, num_angles=10, timeout=UDP_TIMEOUT):
    """
    Send each EMG window via UDP and collect predictions.
    Works identically for local model server and FPGA.
    """
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.settimeout(timeout)
    preds = []
    for i in range(len(emg_windows)):
        payload = emg_windows[i].astype(np.float32).tobytes()
        sock.sendto(payload, server_addr)
        data, _ = sock.recvfrom(num_angles * 4 + 128)
        preds.append(np.frombuffer(data, dtype=np.float32).copy())
    sock.close()
    return np.array(preds)


# ══════════════════════════════════════════════════════════════
#  VISUALISATION
# ══════════════════════════════════════════════════════════════
def plot_emg_signals(raw_emg, processed_emg, n_samples=2000, channels=None):
    if channels is None:
        channels = [0, 3, 7, 11]
    channels = [c for c in channels if c < raw_emg.shape[1]]
    n = min(n_samples, len(raw_emg))

    fig, axes = plt.subplots(len(channels), 2, figsize=(14, 2.5 * len(channels)),
                             sharex=True)
    fig.suptitle("EMG Signals - Before vs After Preprocessing", fontsize=14, y=1.01)
    for i, ch in enumerate(channels):
        axes[i, 0].plot(raw_emg[:n, ch], lw=0.4, color="steelblue")
        axes[i, 0].set_ylabel(f"Ch {ch}")
        if i == 0: axes[i, 0].set_title("Raw EMG")
        axes[i, 1].plot(processed_emg[:n, ch], lw=0.4, color="darkorange")
        if i == 0: axes[i, 1].set_title("After Preprocessing")
    axes[-1, 0].set_xlabel("Sample")
    axes[-1, 1].set_xlabel("Sample")
    plt.tight_layout()
    plt.savefig(os.path.join(os.path.dirname(__file__), "emg_preprocess.png"), dpi=150)
    plt.show()


def plot_predictions(raw_preds, filtered_preds, ground_truth=None):
    """Plot ALL 10 joints: ground truth vs raw vs Kalman."""
    n_joints = raw_preds.shape[1]
    rows, cols = 5, 2
    fig, axes = plt.subplots(rows, cols, figsize=(16, 14), sharex=True)
    fig.suptitle("Predictions - All 10 Joints", fontsize=14, y=1.01)

    for j in range(n_joints):
        ax = axes[j // cols, j % cols]
        if ground_truth is not None:
            ax.plot(ground_truth[:, j], lw=0.8, label="Ground Truth", color="dodgerblue")
        ax.plot(raw_preds[:, j], lw=0.5, alpha=0.7, label="Raw Pred", color="tomato")
        ax.plot(filtered_preds[:, j], lw=1.0, label="Kalman", color="seagreen")
        name = JOINT_NAMES[j] if j < len(JOINT_NAMES) else f"Joint {j}"
        ax.set_ylabel(name, fontsize=8)
        ax.legend(loc="upper right", fontsize=6)

    axes[-1, 0].set_xlabel("Window index")
    axes[-1, 1].set_xlabel("Window index")
    plt.tight_layout()
    plt.savefig(os.path.join(os.path.dirname(__file__), "predictions.png"), dpi=150)
    plt.show()


# ══════════════════════════════════════════════════════════════
#  MAIN PIPELINE
# ══════════════════════════════════════════════════════════════
def run_pipeline():
    timings = {}
    cfg = Config()

    if USE_FPGA:
        server_addr = (FPGA_IP, FPGA_PORT)
    else:
        server_addr = (LOCAL_HOST, LOCAL_PORT)
    print(f"Target: {server_addr}  ({'FPGA' if USE_FPGA else 'Local Model Server'})\n")

    # 1. LOAD
    t0 = time.perf_counter()
    raw_emg, raw_glove, restimulus = load_single_subject_mat(MAT_PATH)
    timings["1_loading"] = time.perf_counter() - t0
    print(f"[1] Loaded  EMG {raw_emg.shape}  Glove {raw_glove.shape}")

    # 2. PREPROCESS
    t0 = time.perf_counter()
    emg = preprocess_emg(raw_emg.copy())
    glove, _, _ = preprocess_glove(raw_glove.copy())
    timings["2_preprocessing"] = time.perf_counter() - t0
    print(f"[2] Preprocessed  EMG {emg.shape}  Glove {glove.shape}")

    plot_emg_signals(raw_emg, emg)

    # 3. SEGMENT
    t0 = time.perf_counter()
    emg_windows, glove_windows, restim_windows = segment_windows(
        emg, glove, restimulus, WINDOW_SIZE, STRIDE)
    timings["3_segmentation"] = time.perf_counter() - t0

    if MAX_WINDOWS is not None:
        emg_windows    = emg_windows[:MAX_WINDOWS]
        glove_windows  = glove_windows[:MAX_WINDOWS]
        restim_windows = restim_windows[:MAX_WINDOWS]
    print(f"[3] Segmented -> {emg_windows.shape[0]} windows")

    # 4. INFERENCE via UDP
    t0 = time.perf_counter()
    print(f"[4] UDP inference ({len(emg_windows)} windows) ...")
    predictions = udp_inference(emg_windows, server_addr, cfg.num_angles)
    timings["4_inference_udp"] = time.perf_counter() - t0
    print(f"    Predictions: {predictions.shape}")

    # 5. KALMAN
    t0 = time.perf_counter()
    filtered = apply_kalman_smoothing(predictions, Q=KALMAN_Q, R=KALMAN_R)
    timings["5_postprocessing"] = time.perf_counter() - t0
    print(f"[5] Kalman applied")

    # Plots: include ground truth
    plot_predictions(predictions, filtered, ground_truth=glove_windows)

    # 6. LATENCY
    total = sum(timings.values())
    print("\n" + "=" * 50)
    print("  LATENCY REPORT")
    print("=" * 50)
    for stage, dt in timings.items():
        pct = 100.0 * dt / total if total > 0 else 0
        print(f"  {stage:<25s}  {dt:8.3f} s   ({pct:5.1f}%)")
    print(f"  {'TOTAL':<25s}  {total:8.3f} s")
    print("=" * 50)

    return predictions, filtered, glove_windows, timings


if __name__ == "__main__":
    # Start local model server if not using FPGA
    if not USE_FPGA:
        from model_server import LocalModelServer
        server = LocalModelServer(host=LOCAL_HOST, port=LOCAL_PORT)
        server.start()
        server.ready.wait(timeout=30)
        print("[Pipeline] Local model server ready.\n")

    run_pipeline()
