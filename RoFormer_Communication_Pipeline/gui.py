"""
sEMG Pipeline GUI

Interactive application for running the RoFormer sEMG-to-joint-angle pipeline.
Supports local model server and FPGA inference, both via UDP.
"""

import os
import sys
import time
import socket
import threading
import serial
import numpy as np
import tkinter as tk
from tkinter import ttk, messagebox

import matplotlib
matplotlib.use("TkAgg")
from matplotlib.figure import Figure
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

# Local imports
from load_dataset import (
    load_single_subject_mat,
    preprocess_emg,
    preprocess_glove,
    segment_windows,
)
from RoFormer import Config
from Kalman import apply_kalman_smoothing, apply_adaptive_kalman_smoothing
from model_server import LocalModelServer
import config

# ── Import configuration ─────────────────────────────────────
MAT_PATH = config.DEFAULT_MAT_PATH
CHECKPOINT = config.CHECKPOINT
WINDOW_SIZE = config.WINDOW_SIZE
STRIDE = config.STRIDE

LOCAL_HOST = config.LOCAL_HOST
LOCAL_PORT = config.LOCAL_PORT
FPGA_IP = config.FPGA_IP
FPGA_PORT = config.FPGA_PORT

KALMAN_Q = config.KALMAN_Q
KALMAN_R = config.KALMAN_R

JOINT_NAMES = config.JOINT_NAMES

# Original hardcoded names (kept for reference)
# JOINT_NAMES = [
#     "Thumb MCP", "Thumb PIP", "Index MCP", "Index PIP",
#     "Middle MCP", "Middle PIP", "Ring MCP", "Ring PIP",
#     "Pinky MCP", "Pinky PIP",
# ]

# GESTURE_LABELS = {
#     0: "Rest",
#     1: "Thumb Up", 2: "Index Extension", 3: "Middle Extension",
#     4: "Ring Extension", 5: "Pinky Extension", 6: "Thumb+Index Pinch",
#     7: "Thumb+Middle Pinch", 8: "Thumb+Ring Pinch", 9: "Thumb+Pinky Pinch",
#     10: "Index+Middle Flex", 11: "Fist", 12: "Pointing Index",
#     13: "Adduction", 14: "Supination", 15: "Pronation",
#     16: "Wrist Flex", 17: "Wrist Extend",
# }

GESTURE_LABELS = {
    0: "Rest",
    1: "Thumb up",
    2: "Extension of index and middle, flexion of the others",
    3: "Flexion of ring and little finger, extension of the others",
    4: "Thumb opposing base of little finger",
    5: "Abduction of all fingers",
    6: "Fingers flexed together in fist",
    7: "Pointing index",
    8: "Adduction of extended fingers",
    9: "Wrist supination (axis: middle finger)",
    10: "Wrist pronation (axis: middle finger)",
    11: "Wrist supination (axis: little finger)",
    12: "Wrist pronation (axis: little finger)",
    13: "Wrist flexion",
    14: "Wrist extension",
    15: "Wrist radial deviation",
    16: "Wrist ulnar deviation",
    17: "Wrist extension with closed hand"
}

# ── Catppuccin Mocha palette ─────────────────────────────────
BG      = "#1e1e2e"
SURFACE = "#313244"
OVERLAY = "#45475a"
TEXT    = "#cdd6f4"
SUBTEXT = "#a6adc8"
ACCENT  = "#89b4fa"
GREEN   = "#a6e3a1"
RED     = "#f38ba8"
YELLOW  = "#f9e2af"
PEACH   = "#fab387"
TEAL    = "#94e2d5"


# ══════════════════════════════════════════════════════════════
#  UDP CLIENT
# ══════════════════════════════════════════════════════════════
def udp_inference(emg_windows, server_addr, num_angles=10, timeout=5.0,
                  progress_cb=None):
    """Send each window via UDP; returns [N, num_angles]."""
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.settimeout(timeout)
    preds = []
    n = len(emg_windows)
    for i in range(n):
        payload = emg_windows[i].astype(np.float32).tobytes()
        sock.sendto(payload, server_addr)
        data, _ = sock.recvfrom(num_angles * 4 + 128)
        preds.append(np.frombuffer(data, dtype=np.float32).copy())
        if progress_cb and i % 50 == 0:
            progress_cb(i, n)
    sock.close()
    return np.array(preds)


def uart_inference(emg_windows, port, baudrate, num_angles=10, timeout=3.0,
                   progress_cb=None):
    """Send each window via UART; returns [N, num_angles]."""
    preds = []
    n = len(emg_windows)
    
    with serial.Serial(port, baudrate=baudrate, timeout=timeout) as ser:
        ser.reset_input_buffer()
        ser.reset_output_buffer()
        for i in range(n):
            payload = emg_windows[i].astype(np.float32).tobytes()
            print(len(payload))
            ser.write(payload)
            ser.flush()  # Block until all bytes leave the PC
            data = ser.read(num_angles * 4)
            print(len(data))
            if len(data) == num_angles * 4:
                preds.append(np.frombuffer(data, dtype=np.float32).copy())
            else:
                raise TimeoutError(f"UART read timed out on window {i}. No response from {port}.")
            
            if progress_cb and i % 50 == 0:
                progress_cb(i, n)

    return np.array(preds)


# ══════════════════════════════════════════════════════════════
#  MAIN GUI
# ══════════════════════════════════════════════════════════════
class PipelineGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("sEMG Pipeline - RoFormer")
        self.root.geometry("1500x920")
        self.root.configure(bg=BG)
        self.root.minsize(1200, 750)

        self.cfg = Config()
        self.server = None          # LocalModelServer instance

        # Pipeline data (cached after first load)
        self.raw_emg = None
        self.processed_emg = None
        self.glove = None
        self.restimulus = None
        self.emg_windows = None
        self.glove_windows = None   # ground truth (end of window)
        self.glove_diff_windows = None
        self.restim_windows = None
        self.available_gestures = []
        self.data_loaded = False

        # Results
        self.predictions = None
        self.filtered = None
        self.current_gt = None
        self.timings = {}

        #Unity Streaming State
        self.is_streaming_unity = False
        self.unity_thread = None

        self._apply_style()
        self._build_ui()

        # Load data on startup (background)
        self.root.after(200, self._load_data_async)

    # ── styling ───────────────────────────────────────────────
    def _apply_style(self):
        s = ttk.Style()
        s.theme_use("clam")
        s.configure(".", background=BG, foreground=TEXT,
                     fieldbackground=SURFACE, borderwidth=0)
        s.configure("TFrame", background=BG)
        s.configure("TLabel", background=BG, foreground=TEXT, font=("Segoe UI", 10))
        s.configure("TLabelframe", background=BG, foreground=ACCENT,
                     font=("Segoe UI", 10, "bold"))
        s.configure("TLabelframe.Label", background=BG, foreground=ACCENT)
        s.configure("Header.TLabel", font=("Segoe UI", 16, "bold"),
                     foreground=ACCENT, background=BG)
        s.configure("Sub.TLabel", font=("Segoe UI", 9), foreground=SUBTEXT,
                     background=BG)
        s.configure("Status.TLabel", font=("Segoe UI", 9, "italic"),
                     foreground=YELLOW, background=SURFACE, padding=6)
        s.configure("Latency.TLabel", font=("Consolas", 10),
                     foreground=TEXT, background=SURFACE)
        s.configure("Run.TButton", font=("Segoe UI", 11, "bold"),
                     foreground=BG, background=GREEN, padding=8)
        s.map("Run.TButton", background=[("active", TEAL)])
        s.configure("TRadiobutton", background=BG, foreground=TEXT,
                     font=("Segoe UI", 10))
        s.configure("TCombobox", fieldbackground=SURFACE, foreground=TEXT,
                     selectbackground=ACCENT)
        s.map("TCombobox", fieldbackground=[("readonly", SURFACE)])

    # ── build layout ──────────────────────────────────────────
    def _build_ui(self):
        # Header
        hdr = ttk.Label(self.root, text="sEMG-to-Joint-Angle Pipeline",
                        style="Header.TLabel")
        hdr.pack(pady=(12, 4))

        # Main paned window: left controls | right content
        pw = ttk.PanedWindow(self.root, orient=tk.HORIZONTAL)
        pw.pack(fill=tk.BOTH, expand=True, padx=10, pady=6)

        # # ── LEFT PANEL (controls + latency) ───────────────────
        # left = ttk.Frame(pw, width=260)
        # pw.add(left, weight=0)

        # ── LEFT PANEL (controls + latency) ───────────────────
        # 1. Create a container for the Canvas and Scrollbar
        left_container = ttk.Frame(pw, width=280)
        pw.add(left_container, weight=0)

        # 2. Create the Canvas
        self.left_canvas = tk.Canvas(left_container, bg=BG, highlightthickness=0)
        self.left_canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # 3. Create and attach the Scrollbar
        scrollbar = ttk.Scrollbar(left_container, orient=tk.VERTICAL, command=self.left_canvas.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.left_canvas.configure(yscrollcommand=scrollbar.set)

        # 4. Create the actual 'left' frame INSIDE the canvas
        left = ttk.Frame(self.left_canvas)
        
        # 5. Add the frame to a window in the canvas (width=260 forces the column width)
        self.left_canvas.create_window((0, 0), window=left, anchor="nw", width=260)

        # 6. Update the scrollable area when widgets are added to 'left'
        def _on_frame_configure(event):
            self.left_canvas.configure(scrollregion=self.left_canvas.bbox("all"))
        left.bind("<Configure>", _on_frame_configure)

        # 7. Add Mouse-wheel scrolling
        def _on_mousewheel(event):
            # event.delta is standard for Windows/macOS
            self.left_canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")
            
        # Bind mousewheel when hovering over the left panel
        left_container.bind("<Enter>", lambda e: self.root.bind_all("<MouseWheel>", _on_mousewheel))
        left_container.bind("<Leave>", lambda e: self.root.unbind_all("<MouseWheel>"))
    
        # Mode selection
        mode_frame = ttk.LabelFrame(left, text="  Inference Mode  ", padding=10)
        mode_frame.pack(fill=tk.X, padx=6, pady=(6, 4))

        self.mode_var = tk.StringVar(value="local")
        ttk.Radiobutton(mode_frame, text="Local Model",
                        variable=self.mode_var, value="local").pack(anchor=tk.W)
        ttk.Radiobutton(mode_frame, text="FPGA",
                        variable=self.mode_var, value="fpga").pack(anchor=tk.W, pady=(4,0))

        # FPGA IP entry
        ip_frame = ttk.Frame(mode_frame)
        ip_frame.pack(fill=tk.X, pady=(6, 0))
        ttk.Label(ip_frame, text="FPGA IP:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.fpga_ip_var = tk.StringVar(value=FPGA_IP)
        ttk.Entry(ip_frame, textvariable=self.fpga_ip_var, width=16).pack(
            side=tk.LEFT, padx=4)

        # Protocol selection
        proto_frame = ttk.LabelFrame(left, text="  Protocol  ", padding=10)
        proto_frame.pack(fill=tk.X, padx=6, pady=4)

        self.proto_var = tk.StringVar(value="udp")
        ttk.Radiobutton(proto_frame, text="UDP",
                        variable=self.proto_var, value="udp").pack(anchor=tk.W)
        ttk.Radiobutton(proto_frame, text="UART",
                        variable=self.proto_var, value="uart").pack(anchor=tk.W, pady=(4,0))

        com_frame = ttk.Frame(proto_frame)
        com_frame.pack(fill=tk.X, pady=(6, 0))
        
        ttk.Label(com_frame, text="Client:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.com_port_var = tk.StringVar(value="COM5")
        ttk.Entry(com_frame, textvariable=self.com_port_var, width=6).pack(side=tk.LEFT, padx=(2, 6))

        ttk.Label(com_frame, text="Server:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.server_com_port_var = tk.StringVar(value="COM6")
        ttk.Entry(com_frame, textvariable=self.server_com_port_var, width=6).pack(side=tk.LEFT, padx=(2, 6))

        ttk.Label(com_frame, text="Baud:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.baud_var = tk.StringVar(value="115200")
        ttk.Entry(com_frame, textvariable=self.baud_var, width=8).pack(side=tk.LEFT, padx=(2, 0))

        # Gesture selector
        gest_frame = ttk.LabelFrame(left, text="  Gesture (restimulus)  ", padding=10)
        gest_frame.pack(fill=tk.X, padx=6, pady=4)

        self.gesture_var = tk.StringVar(value="All")
        self.gesture_combo = ttk.Combobox(gest_frame, textvariable=self.gesture_var,
                                          state="readonly", width=26)
        self.gesture_combo.pack(fill=tk.X)

        # Max windows
        win_frame = ttk.LabelFrame(left, text="  Max Windows  ", padding=10)
        win_frame.pack(fill=tk.X, padx=6, pady=4)
        self.max_win_var = tk.StringVar(value="500")
        ttk.Entry(win_frame, textvariable=self.max_win_var, width=10).pack(anchor=tk.W)

        # Filter selection
        filter_frame = ttk.LabelFrame(left, text="  Post-Processing Filter  ", padding=10)
        filter_frame.pack(fill=tk.X, padx=6, pady=4)

        self.filter_var = tk.StringVar(value="kalman")
        ttk.Radiobutton(filter_frame, text="Kalman Filter",
                        variable=self.filter_var, value="kalman").pack(anchor=tk.W)
        ttk.Radiobutton(filter_frame, text="Adaptive Kalman Filter",
                        variable=self.filter_var, value="adaptive").pack(anchor=tk.W, pady=(4,0))

        # Run button
        self.run_btn = ttk.Button(left, text="  \u25B6  RUN PIPELINE  ",
                                  style="Run.TButton",
                                  command=self._on_run)
        self.run_btn.pack(fill=tk.X, padx=6, pady=10)

        # Latency & Metrics panel
        lat_frame = ttk.LabelFrame(left, text="  Latency & Metrics  ", padding=10)
        lat_frame.pack(fill=tk.BOTH, expand=True, padx=6, pady=4)

        self.latency_text = tk.Text(lat_frame, bg=SURFACE, fg=TEXT,
                                    font=("Consolas", 10), height=12,
                                    bd=0, highlightthickness=0,
                                    state=tk.DISABLED, wrap=tk.NONE)
        self.latency_text.pack(fill=tk.BOTH, expand=True)


        # ── Unity Streaming Panel ─────────────────────────
        unity_frame = ttk.LabelFrame(left, text="  Unity Streaming  ", padding=10)
        unity_frame.pack(fill=tk.X, padx=6, pady=4)

        u_ip_frame = ttk.Frame(unity_frame)
        u_ip_frame.pack(fill=tk.X, pady=(0, 4))
        ttk.Label(u_ip_frame, text="IP:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.unity_ip_var = tk.StringVar(value="127.0.0.1")
        ttk.Entry(u_ip_frame, textvariable=self.unity_ip_var, width=12).pack(side=tk.LEFT, padx=(4, 10))

        ttk.Label(u_ip_frame, text="Port:", font=("Segoe UI", 9)).pack(side=tk.LEFT)
        self.unity_port_var = tk.StringVar(value="8051")
        ttk.Entry(u_ip_frame, textvariable=self.unity_port_var, width=6).pack(side=tk.LEFT, padx=(4, 0))

        self.stream_btn = ttk.Button(unity_frame, text=" ▶ Start Unity Stream ",
                                     command=self._toggle_unity_stream)
        self.stream_btn.pack(fill=tk.X, pady=(6, 0))
        # ──────────────────────────────────────────────────────

        # Status bar
        self.status_var = tk.StringVar(value="Loading data ...")
        ttk.Label(left, textvariable=self.status_var,
                  style="Status.TLabel").pack(fill=tk.X, padx=6, pady=(4, 6))

        # ── RIGHT PANEL (notebook with plots) ─────────────────
        right = ttk.Frame(pw)
        pw.add(right, weight=1)

        self.notebook = ttk.Notebook(right)
        self.notebook.pack(fill=tk.BOTH, expand=True)

        # Tab 1: EMG signals
        self.emg_tab = ttk.Frame(self.notebook)
        self.notebook.add(self.emg_tab, text="  EMG Signals  ")
        self.emg_fig = Figure(figsize=(12, 7), facecolor=BG)
        self.emg_canvas = FigureCanvasTkAgg(self.emg_fig, master=self.emg_tab)
        self.emg_canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)

        # Tab 2: Predictions
        self.pred_tab = ttk.Frame(self.notebook)
        self.notebook.add(self.pred_tab, text="  Predictions (10 Joints)  ")
        self.pred_fig = Figure(figsize=(12, 7), facecolor=BG)
        self.pred_canvas = FigureCanvasTkAgg(self.pred_fig, master=self.pred_tab)
        self.pred_canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)

    # ── data loading (background) ─────────────────────────────
    def _load_data_async(self):
        threading.Thread(target=self._load_data, daemon=True).start()

    def _load_data(self):
        try:
            t0 = time.perf_counter()

            # 1. Load raw .mat
            self.raw_emg, raw_glove, self.restimulus = \
                load_single_subject_mat(MAT_PATH)
            self.timings["1_loading"] = time.perf_counter() - t0

            # 2. Preprocess
            t1 = time.perf_counter()
            self.processed_emg = preprocess_emg(self.raw_emg.copy())
            self.glove, _, _ = preprocess_glove(raw_glove.copy())
            self.timings["2_preprocess"] = time.perf_counter() - t1

            # 3. Segment
            t2 = time.perf_counter()
            self.emg_windows, self.glove_windows, self.glove_diff_windows, self.restim_windows = \
                segment_windows(self.processed_emg, self.glove,
                                self.restimulus, WINDOW_SIZE, STRIDE)
            self.timings["3_segment"] = time.perf_counter() - t2

            # Find available gestures
            unique = sorted(np.unique(self.restim_windows).tolist())
            self.available_gestures = unique
            labels = ["All"] + [
                f"{g} - {GESTURE_LABELS.get(g, f'Gesture {g}')}" for g in unique
            ]
            self.data_loaded = True

            # Update GUI from main thread
            self.root.after(0, lambda: self._on_data_loaded(labels))

        except Exception as e:
            self.root.after(0, lambda: self._set_status(f"ERROR: {e}"))

    def _on_data_loaded(self, labels):
        self.gesture_combo["values"] = labels
        self.gesture_combo.current(0)
        self._set_status(
            f"Data loaded: {self.emg_windows.shape[0]} total windows  "
            f"({self.raw_emg.shape[0]:,} samples)")
        self._update_latency()
        self._plot_emg()

    # ── EMG plot ──────────────────────────────────────────────
    def _plot_emg(self):
        if self.raw_emg is None:
            return
        self.emg_fig.clear()
        channels = [0, 3, 7, 11]
        n = min(4000, len(self.raw_emg))

        for i, ch in enumerate(channels):
            # Raw
            ax_raw = self.emg_fig.add_subplot(len(channels), 2, 2 * i + 1)
            ax_raw.plot(self.raw_emg[:n, ch], lw=0.3, color="#89b4fa")
            ax_raw.set_ylabel(f"Ch {ch}", fontsize=8, color=TEXT)
            ax_raw.tick_params(colors=SUBTEXT, labelsize=7)
            ax_raw.set_facecolor(SURFACE)
            if i == 0:
                ax_raw.set_title("Raw EMG", fontsize=10, color=ACCENT)

            # Processed
            ax_proc = self.emg_fig.add_subplot(len(channels), 2, 2 * i + 2)
            ax_proc.plot(self.processed_emg[:n, ch], lw=0.3, color="#fab387")
            ax_proc.tick_params(colors=SUBTEXT, labelsize=7)
            ax_proc.set_facecolor(SURFACE)
            if i == 0:
                ax_proc.set_title("After Preprocessing", fontsize=10, color=PEACH)

        self.emg_fig.tight_layout(pad=1.5)
        self.emg_canvas.draw()

    # ── predictions plot ──────────────────────────────────────
    def _plot_predictions(self, gt, raw_pred, kalman_pred, filter_name="Kalman"):
        self.pred_fig.clear()
        n_joints = raw_pred.shape[1]
        # normalize raw_pred
        raw_pred = (raw_pred - np.min(raw_pred, axis=0)) / (np.max(raw_pred, axis=0) - np.min(raw_pred, axis=0))
        kalman_pred = (kalman_pred - np.min(kalman_pred, axis=0)) / (np.max(kalman_pred, axis=0) - np.min(kalman_pred, axis=0))
        rows, cols = 5, 2

        for j in range(n_joints):
            ax = self.pred_fig.add_subplot(rows, cols, j + 1)
            ax.plot(gt[:, j], lw=0.8, label="Ground Truth", color="#89b4fa")
            ax.plot(raw_pred[:, j], lw=0.5, alpha=0.7, label="Raw Pred",
                    color="#f38ba8")
            ax.plot(kalman_pred[:, j], lw=1.0, label=filter_name, color="#a6e3a1")
            name = JOINT_NAMES[j] if j < len(JOINT_NAMES) else f"Joint {j}"
            ax.set_ylabel(name, fontsize=7, color=TEXT)
            ax.tick_params(colors=SUBTEXT, labelsize=6)
            ax.set_facecolor(SURFACE)
            if j < 2:
                ax.legend(fontsize=6, loc="upper right",
                          facecolor=OVERLAY, edgecolor=OVERLAY, labelcolor=TEXT)

        self.pred_fig.tight_layout(pad=1.0)
        self.pred_canvas.draw()

    # ── latency display ───────────────────────────────────────
    def _update_latency(self):
        self.latency_text.configure(state=tk.NORMAL)
        self.latency_text.delete("1.0", tk.END)

        total = sum(self.timings.values())
        lines = []
        for stage, dt in self.timings.items():
            pct = 100 * dt / total if total > 0 else 0
            lines.append(f"  {stage:<22s} {dt:7.3f}s  ({pct:4.1f}%)")
        lines.append(f"  {'TOTAL':<22s} {total:7.3f}s")
        self.latency_text.insert(tk.END, "\n".join(lines))
        self.latency_text.configure(state=tk.DISABLED)

    # ── status ────────────────────────────────────────────────
    def _set_status(self, msg):
        self.status_var.set(msg)


    # ── Unity Streaming Logic ─────────────────────────────────
    def _toggle_unity_stream(self):
        if self.predictions is None or self.current_gt is None:
            messagebox.showwarning("No Data", "Please run the pipeline first!")
            return

        if self.is_streaming_unity:
            # Stop streaming
            self.is_streaming_unity = False
            self.stream_btn.configure(text=" ▶ Start Unity Stream ")
            self._set_status("Unity streaming stopped.")
        else:
            # Start streaming
            self.is_streaming_unity = True
            self.stream_btn.configure(text=" ⏹ Stop Unity Stream ")
            self.unity_thread = threading.Thread(target=self._unity_worker, daemon=True)
            self.unity_thread.start()

    def _unity_worker(self):
        ip = self.unity_ip_var.get()
        port = int(self.unity_port_var.get())
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        # Decide which prediction array to use
        preds = self.filtered if self.filtered is not None else self.predictions
        gt = self.current_gt

        total_frames = len(preds)
        idx = 0

        self.root.after(0, lambda: self._set_status(f"Streaming {total_frames} frames to Unity..."))

        try:
            while self.is_streaming_unity:
                # 1. Extract the 10 joints for the current frame
                gt_10 = gt[idx]
                pred_10 = preds[idx]

                # 2. Pad to 12 floats (adds two 0.0s at the end) to match Unity expectations
                gt_12 = np.pad(gt_10, (0, 2), mode='constant')
                pred_12 = np.pad(pred_10, (0, 2), mode='constant')

                # 3. Combine into one 24-float array: [12 Ground Truth, 12 Predicted]
                combined = np.concatenate((gt_12, pred_12)).astype(np.float32)

                # 4. Send as raw binary bytes
                sock.sendto(combined.tobytes(), (ip, port))

                # Loop back to the start if we reach the end of the sequence
                idx += 1
                if idx >= total_frames:
                    idx = 0

                # 50ms delay = 20 FPS
                time.sleep(0.05) 
                
        except Exception as e:
            print(f"Unity UDP Error: {e}")
        finally:
            sock.close()
            self.is_streaming_unity = False
            self.root.after(0, lambda: self.stream_btn.configure(text=" ▶ Start Unity Stream "))

    # ── RUN button handler ────────────────────────────────────
    def _on_run(self):
        if not self.data_loaded:
            messagebox.showwarning("Not Ready", "Data is still loading.")
            return
        self.run_btn.configure(state=tk.DISABLED)
        self._set_status("Running pipeline ...")
        threading.Thread(target=self._run_pipeline, daemon=True).start()

    def _run_pipeline(self):
        try:
            cfg = self.cfg

            # ── Filter by selected gesture ────────────────────
            sel = self.gesture_var.get()
            if sel == "All":
                mask = np.ones(len(self.restim_windows), dtype=bool)
            else:
                gest_id = int(sel.split(" - ")[0])
                mask = self.restim_windows == gest_id

            emg_w = self.emg_windows[mask]
            glove_w = self.glove_windows[mask]
            glove_diff_w = self.glove_diff_windows[mask]

            # ── Cap windows ───────────────────────────────────
            try:
                max_w = int(self.max_win_var.get())
            except ValueError:
                max_w = 500
            emg_w = emg_w[:max_w]
            glove_w = glove_w[:max_w]
            glove_diff_w = glove_diff_w[:max_w]

            if len(emg_w) == 0:
                self.root.after(0, lambda: messagebox.showwarning(
                    "No Data", "No windows for the selected gesture."))
                return

            self.root.after(0, lambda: self._set_status(
                f"Inference on {len(emg_w)} windows ..."))

            # ── Determine server address ──────────────────────
            use_fpga = self.mode_var.get() == "fpga"
            protocol = self.proto_var.get()

            if use_fpga:
                addr = (self.fpga_ip_var.get(), FPGA_PORT)
            else:
                addr = (LOCAL_HOST, LOCAL_PORT)
                baud = int(self.baud_var.get())
                server_com = self.server_com_port_var.get() if hasattr(self, 'server_com_port_var') else "COM4"
                self._ensure_model_server(protocol=protocol, com_port=server_com, baudrate=baud)

            # ── 4. Inference ──────────────────────────────
            t0 = time.perf_counter()

            if protocol == "udp":
                predictions = udp_inference(
                    emg_w, addr, cfg.num_angles,
                    progress_cb=lambda i, n: self.root.after(
                        0, lambda: self._set_status(
                            f"Inference: {i}/{n} windows ...")))
                self.timings["4_inference_udp"] = time.perf_counter() - t0
            else:
                port = self.com_port_var.get()
                baud = int(self.baud_var.get())
                predictions = uart_inference(
                    emg_w, port, baud, cfg.num_angles,
                    progress_cb=lambda i, n: self.root.after(
                        0, lambda: self._set_status(
                            f"Inference: {i}/{n} windows ...")))
                self.timings["4_inference_uart"] = time.perf_counter() - t0

            # ── 5. Reconstruct Absolute Angles ────────────────
            # The model predicts the angle difference scaled by 100.
            pred_diffs_real = predictions / 100.0
            
            # Ground truth start angles for these windows
            start_angles = glove_w - glove_diff_w
            
            # Predicted end angles
            raw_pred_angles = start_angles + pred_diffs_real

            # ── 6. Post-Processing Filter ─────────────────────
            t0 = time.perf_counter()
            if self.filter_var.get() == "adaptive":
                filtered = apply_adaptive_kalman_smoothing(raw_pred_angles, Q=KALMAN_Q, R=KALMAN_R)
            else:
                filtered = apply_kalman_smoothing(raw_pred_angles, Q=KALMAN_Q, R=KALMAN_R)
            self.timings["5_filter"] = time.perf_counter() - t0

            self.predictions = raw_pred_angles
            self.filtered = filtered
            self.current_gt = glove_w

            # ── Update GUI on main thread ─────────────────────
            filter_name = "Adaptive Kalman" if self.filter_var.get() == "adaptive" else "Kalman"
            self.root.after(0, lambda: self._on_pipeline_done(
                glove_w, raw_pred_angles, filtered, filter_name))

        except Exception as e:
            self.root.after(0, lambda: self._set_status(f"ERROR: {e}"))
            import traceback; traceback.print_exc()
        finally:
            self.root.after(0, lambda: self.run_btn.configure(state=tk.NORMAL))

    def _on_pipeline_done(self, gt, raw_pred, kalman_pred, filter_name="Kalman"):
        self._update_latency()
        self._plot_predictions(gt, raw_pred, kalman_pred, filter_name)
        
        # Calculate overall RMSE
        rmse_raw = np.sqrt(np.mean((gt - raw_pred)**2))
        rmse_kalman = np.sqrt(np.mean((gt - kalman_pred)**2))
        
        # Calculate average R2 score (sklearn style)
        ss_tot = np.sum((gt - np.mean(gt, axis=0))**2, axis=0)
        ss_res_raw = np.sum((gt - raw_pred)**2, axis=0)
        r2_raw = np.mean(1 - (ss_res_raw / (ss_tot + 1e-8)))
        
        ss_res_kalman = np.sum((gt - kalman_pred)**2, axis=0)
        r2_kalman = np.mean(1 - (ss_res_kalman / (ss_tot + 1e-8)))

        # Append to text panel
        self.latency_text.configure(state=tk.NORMAL)
        self.latency_text.insert(tk.END, "\n\n  --- METRICS ---\n")
        self.latency_text.insert(tk.END, f"  Raw RMSE: {rmse_raw:.4f}\n")
        self.latency_text.insert(tk.END, f"  Raw R2:   {r2_raw:.4f}\n")
        self.latency_text.insert(tk.END, f"  Fltr RMSE:{rmse_kalman:.4f}\n")
        self.latency_text.insert(tk.END, f"  Fltr R2:  {r2_kalman:.4f}\n")
        self.latency_text.configure(state=tk.DISABLED)

        self.notebook.select(self.pred_tab)
        self._set_status(
            f"Done! {raw_pred.shape[0]} windows. {filter_name} RMSE: {rmse_kalman:.4f}")

    # ── model server management ───────────────────────────────
    def _ensure_model_server(self, protocol="udp", com_port="COM4", baudrate=115200):
        """Start the local model server if not already running, or if config changed."""
        needs_restart = False
        if self.server is not None:
            if not self.server.running:
                needs_restart = True
            elif getattr(self.server, 'protocol', 'udp') != protocol:
                needs_restart = True
            elif protocol == "uart" and (getattr(self.server, 'com_port', '') != com_port or getattr(self.server, 'baudrate', 0) != baudrate):
                needs_restart = True
                
        if needs_restart:
            self.server.stop()
            self.server.join(timeout=2.0)
            self.server = None

        if self.server is None:
            self._set_status(f"Starting local model server ({protocol.upper()}) ...")
            self.server = LocalModelServer(
                checkpoint=CHECKPOINT, host=LOCAL_HOST, port=LOCAL_PORT,
                protocol=protocol, com_port=com_port, baudrate=baudrate)
            self.server.start()
            self.server.ready.wait(timeout=30)

    # ── cleanup ───────────────────────────────────────────────
    def on_close(self):
        if self.server is not None:
            self.server.stop()
        self.root.destroy()


# ══════════════════════════════════════════════════════════════
if __name__ == "__main__":
    root = tk.Tk()
    app = PipelineGUI(root)
    root.protocol("WM_DELETE_WINDOW", app.on_close)
    root.mainloop()
