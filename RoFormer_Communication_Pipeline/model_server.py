"""
Local RoFormer UDP inference server.

Listens for EMG windows (float32 blobs) on a UDP port,
runs the model, and sends predictions back to the sender.
Can run standalone or be started as a daemon thread from the GUI.
"""

import os
import socket
import threading
import numpy as np
import torch
import serial

from RoFormer import load_roformer, Config
import config

CHECKPOINT = config.CHECKPOINT


class LocalModelServer(threading.Thread):
    """UDP/UART server wrapping local RoFormer inference."""

    def __init__(self, checkpoint=CHECKPOINT, host=config.LOCAL_HOST, port=config.LOCAL_PORT,
                 protocol=config.LOCAL_PROTOCOL, com_port=config.SERIAL_PORT, 
                 baudrate=config.SERIAL_BAUDRATE):
        super().__init__(daemon=True)
        self.cfg = Config()
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.model = load_roformer(checkpoint, self.cfg, self.device)
        
        self.protocol = protocol
        self.host = host
        self.port = port
        self.com_port = com_port
        self.baudrate = baudrate
        
        self.running = False
        self.sock = None
        self.ser = None
        self.ready = threading.Event()

    # ── thread entry ──────────────────────────────────────────
    def run(self):
        self.running = True
        if self.protocol == "udp":
            self.run_udp()
        else:
            self.run_uart()

    def run_udp(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind((self.host, self.port))
        self.sock.settimeout(1.0)
        self.ready.set()
        print(f"[ModelServer] Listening on UDP {self.host}:{self.port} (device={self.device})")

        while self.running:
            try:
                data, addr = self.sock.recvfrom(65536)
                arr = np.frombuffer(data, dtype=np.float32).copy()
                window = arr.reshape(self.cfg.max_seq_len, self.cfg.num_channels)

                tensor = (torch.tensor(window.T, dtype=torch.float32)
                          .unsqueeze(0).to(self.device))
                with torch.no_grad():
                    pred = self.model(tensor).cpu().numpy().flatten()

                self.sock.sendto(pred.astype(np.float32).tobytes(), addr)
            except socket.timeout:
                continue
            except Exception as e:
                print(f"[ModelServer] UDP Error: {e}")

        self.sock.close()
        print("[ModelServer] UDP Stopped.")

    def run_uart(self):
        try:
            self.ser = serial.Serial(self.com_port, self.baudrate, timeout=1.0)
        except Exception as e:
            print(f"[ModelServer] Failed to open UART {self.com_port}: {e}")
            self.running = False
            self.ready.set()
            return

        self.ready.set()
        print(f"[ModelServer] Listening on UART {self.com_port}@{self.baudrate} (device={self.device})")
        
        expected_bytes = self.cfg.max_seq_len * self.cfg.num_channels * 4
        
        while self.running:
            try:
                data = self.ser.read(expected_bytes)
                if len(data) == expected_bytes:
                    arr = np.frombuffer(data, dtype=np.float32).copy()
                    window = arr.reshape(self.cfg.max_seq_len, self.cfg.num_channels)

                    tensor = (torch.tensor(window.T, dtype=torch.float32)
                              .unsqueeze(0).to(self.device))
                    with torch.no_grad():
                        pred = self.model(tensor).cpu().numpy().flatten()

                    self.ser.write(pred.astype(np.float32).tobytes())
            except Exception as e:
                print(f"[ModelServer] UART Error: {e}")

        self.ser.close()
        print("[ModelServer] UART Stopped.")

    def stop(self):
        self.running = False


# ── standalone entry point ────────────────────────────────────
if __name__ == "__main__":
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("--host", default="127.0.0.1")
    ap.add_argument("--port", type=int, default=5005)
    args = ap.parse_args()

    server = LocalModelServer(host=args.host, port=args.port)
    server.start()
    try:
        server.join()           # block forever
    except KeyboardInterrupt:
        server.stop()
