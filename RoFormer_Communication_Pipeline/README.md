# RoFormer sEMG-to-Joint-Angle Pipeline

A complete end-to-end pipeline for predicting hand joint angles from Surface Electromyography (sEMG) signals using a RoFormer (Rotary Position Embedding Transformer) model. Supports local model inference and FPGA deployment via UDP/UART communication.

## 📋 Project Overview

This capstone project implements a real-time sEMG signal processing pipeline that:
- ✅ Preprocesses and segments raw EMG data
- ✅ Runs inference using a pre-trained RoFormer model
- ✅ Applies Kalman filtering for signal smoothing
- ✅ Provides both GUI and command-line interfaces
- ✅ Supports multiple inference backends (local CPU/GPU, FPGA via UDP)
- ✅ Sends predictions to Unity for visualization

## ✨ Features

- 🎯 **RoFormer Model**: Transformer-based architecture with Rotary Position Embeddings
- 📊 **Interactive GUI**: tkinter application with real-time visualization and parameter control
- 🔄 **Multiple Backends**: Local model server, FPGA via UDP, or UART serial
- 🎚️ **Adaptive Kalman Filter**: Automatic noise reduction based on signal characteristics
- 🔌 **Flexible Communication**: UDP and serial (UART) support for different hardware setups
- 📈 **Live Plots**: Real-time EMG signal and joint angle visualization
- 🤚 **10 DOF Hand Model**: MCP and PIP joints for complete hand representation

## 📁 Project Structure

```
RoFormer_Communication_Pipeline/
├── gui.py                    # Interactive tkinter GUI application
├── pipeline.py               # End-to-end inference pipeline
├── model_server.py           # Local UDP/UART inference server
├── RoFormer.py               # Model architecture and utilities
├── load_dataset.py           # Data loading and preprocessing
├── Kalman.py                 # Kalman filtering implementation
├── config.py                 # Centralized configuration (IMPORTANT!)
├── roformer.pt               # Pre-trained model checkpoint (~200MB)
├── best_roformer/            # HuggingFace model format (alternative)
├── requirements.txt          # Python dependencies
├── README.md                 # This file
├── SETUP.md                  # Quick start guide
└── [generated files]
    ├── emg_preprocess.png    # Signal preprocessing visualization
    ├── predictions.png       # Joint angle predictions
    └── actual_angles.txt     # Ground truth angles
```

## 📋 Requirements

### System Requirements
- **OS**: Windows, macOS, or Linux
- **Python**: 3.8 or higher
- **RAM**: 4GB minimum (8GB recommended for GPU)
- **Storage**: 500MB for dependencies + 200MB for model weights

### Python Packages

| Package | Version |
|---------|---------|
| Python | 3.8+ |
| PyTorch | 2.0+ |
| NumPy | 1.21+ |
| SciPy | 1.7+ |
| Matplotlib | 3.4+ |
| scikit-learn | 0.24+ |
| pyserial | 3.5+ |

Full list in `requirements.txt`

## 🚀 Installation & Setup

### Step 1: Clone the Repository
```bash
git clone https://github.com/yourusername/RoFormer_Communication_Pipeline.git
cd RoFormer_Communication_Pipeline
```

### Step 2: Create Virtual Environment

**On Windows:**
```bash
python -m venv venv
venv\Scripts\activate
```

**On macOS/Linux:**
```bash
python3 -m venv venv
source venv/bin/activate
```

### Step 3: Install Dependencies
```bash
pip install --upgrade pip
pip install -r requirements.txt
```

> **Note for GPU users**: If you have CUDA 11.8+, you can install the GPU version of PyTorch:
> ```bash
> pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
> ```

### Step 4: Download the Model

The pre-trained RoFormer model weights are required to run the pipeline.

**Option A: Direct Download**
1. Download `roformer.pt` from [GitHub Releases](https://github.com/yourusername/releases)
2. Place it in the project root directory:
   ```
   RoFormer_Communication_Pipeline/
   ├── roformer.pt  ← Place here
   ├── gui.py
   └── ...
   ```

**Option B: Using Environment Variable**
```bash
export ROFORMER_CHECKPOINT="/path/to/roformer.pt"  # macOS/Linux
set ROFORMER_CHECKPOINT=C:\path\to\roformer.pt     # Windows
```

**Option C: Load from HuggingFace**
Model can also be loaded from the `best_roformer/` directory if available.

## 📖 Quick Start Guide

### Running the GUI Application
```bash
python gui.py
```

**GUI Features:**
- Load EMG data from .mat files
- Real-time joint angle prediction
- Toggle between local model and FPGA inference
- Adjust Kalman filter parameters
- Visualize EMG signals and predictions
- Export results to files

### Running the Model Server (Standalone)
To run the inference server without the GUI:

```bash
python model_server.py
```

The server will start listening on the configured UDP port (default: 127.0.0.1:5005)

### Running the End-to-End Pipeline
For batch processing of EMG data:

```bash
python pipeline.py --mat-path "path/to/data.mat" --output-file "predictions.txt"
```

**Arguments:**
- `--mat-path`: Path to input EMG data (MAT format)
- `--output-file`: Output file for predictions
- `--use-fpga`: Use FPGA instead of local model (default: False)
- `--kalman-q`: Process noise covariance (default: 1e-4)
- `--kalman-r`: Measurement noise covariance (default: 0.05)

## 🔧 Configuration

### Using config.py

All settings are centralized in `config.py`. Edit this file to customize:

```python
# Project Paths
CHECKPOINT = "roformer.pt"
DEFAULT_MAT_PATH = None  # Or provide your data path

# EMG Processing
WINDOW_SIZE = 400          # samples @ 2kHz = 200ms
STRIDE = 50                # sliding window stride
SAMPLING_RATE = 2000       # Hz

# Network Configuration
LOCAL_HOST = "127.0.0.1"
LOCAL_PORT = 5005
FPGA_IP = "192.168.1.10"
FPGA_PORT = 5005

# Kalman Filter
KALMAN_Q = 1e-4
KALMAN_R = 0.05
KALMAN_USE_ADAPTIVE = True

# Device
DEVICE = "cuda" if GPU available else "cpu"
```

### Using Environment Variables (Override config.py)

```bash
# Linux/macOS
export ROFORMER_CHECKPOINT="/path/to/model.pt"
export ROFORMER_MAT_PATH="/path/to/data.mat"
export ROFORMER_DEVICE="cuda"
export ROFORMER_FPGA_IP="192.168.1.10"
export ROFORMER_LOCAL_PORT="5005"

# Windows (PowerShell)
$env:ROFORMER_CHECKPOINT="C:\path\to\model.pt"
$env:ROFORMER_DEVICE="cuda"
```

## 🔌 Connection Configuration

### For Local CPU/GPU Inference
```python
USE_FPGA = False
LOCAL_HOST = "127.0.0.1"
LOCAL_PORT = 5005
```

### For FPGA Deployment
```python
USE_FPGA = True
FPGA_IP = "192.168.1.10"  # Your FPGA board IP
FPGA_PORT = 5005
```

### For Serial (UART) Communication
```python
LOCAL_PROTOCOL = "uart"  # instead of "udp"
SERIAL_PORT = "COM4"      # Windows: COM3/COM4, macOS/Linux: /dev/ttyUSB0
SERIAL_BAUDRATE = 115200
```

## 📊 Data Format

### Input EMG Data (MAT File)
Expected structure:
```matlab
% data.mat should contain:
emg_raw       % [N_samples × N_channels] - Raw sEMG signal (12 channels)
glove_angles  % [N_samples × 10] - Ground truth joint angles (optional)
%             % Joints: [Thumb MCP, Thumb PIP, Index MCP, Index PIP, ...]
```

**Example Python code to save MAT file:**
```python
import scipy.io as sio

data = {
    'emg_raw': emg_array,        # Shape: [10000, 12]
    'glove_angles': angles_array # Shape: [10000, 10]
}
sio.savemat('data.mat', data)
```

### Output Format
Joint angles in order:
1. Thumb MCP
2. Thumb PIP
3. Index MCP
4. Index PIP
5. Middle MCP
6. Middle PIP
7. Ring MCP
8. Ring PIP
9. Pinky MCP
10. Pinky PIP

Values are in **radians**. Convert to degrees: `angle_deg = angle_rad * 180 / π`

## 🔄 UDP Communication Protocol

### Message Flow
```
1. Client sends EMG window (float32 array, 400 samples)
2. Server receives and processes
3. Server sends 10 joint angles back (float32 array)
```

### Packet Format
- **Request**: 1600 bytes (400 float32 values)
- **Response**: 40 bytes (10 float32 values)
- **Protocol**: UDP
- **Timeout**: 5 seconds (configurable)

### Python Example
```python
import socket
import numpy as np

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server = ("127.0.0.1", 5005)

# Send EMG window
emg_window = np.random.randn(400).astype(np.float32)
sock.sendto(emg_window.tobytes(), server)

# Receive predictions
angles, _ = sock.recvfrom(40)
angles = np.frombuffer(angles, dtype=np.float32)
print("Joint angles:", angles)

sock.close()
```

## ⚠️ Troubleshooting

### Installation Issues

**"ModuleNotFoundError: No module named 'torch'"**
```bash
# Ensure virtual environment is activated
# Windows
venv\Scripts\activate
# macOS/Linux
source venv/bin/activate

# Reinstall PyTorch
pip install torch==2.0.0 torchvision torchaudio
```

**"ModuleNotFoundError: No module named 'tkinter'"**
```bash
# Ubuntu/Debian
sudo apt-get install python3-tk

# macOS
brew install python-tk

# Windows: Reinstall Python with tcl/tk option
```

### Runtime Issues

**"Cannot find roformer.pt"**
- Verify file exists in project root: `ls roformer.pt`
- Check ROFORMER_CHECKPOINT environment variable
- Download from GitHub Releases and place in root directory

**"CUDA out of memory"**
- Reduce WINDOW_SIZE in config.py
- Set DEVICE to "cpu" in config.py
- Close other GPU-intensive applications

**"UDP: Connection refused"**
- Ensure model server is running (see Quick Start)
- Check firewall settings (Windows/macOS)
- Verify UDP port is not in use: `netstat -an | grep 5005`

**"Socket timeout"**
- Check if model server is running
- Increase UDP_TIMEOUT in config.py
- Verify network connectivity

### Kalman Filter Tuning

**Signal too noisy?** → Decrease KALMAN_R (smoother)
```python
KALMAN_R = 0.01  # Increase smoothing
```

**Not following signal changes?** → Increase KALMAN_Q (faster response)
```python
KALMAN_Q = 1e-3  # Faster response
```

### GPU Issues

**Check if GPU is detected:**
```python
import torch
print("GPU Available:", torch.cuda.is_available())
print("GPU Name:", torch.cuda.get_device_name(0) if torch.cuda.is_available() else "None")
```

**Force CPU:**
```bash
export ROFORMER_DEVICE="cpu"
```

## 📈 Performance Benchmarks

### Inference Speed (on RTX 3060 / i7-10700)
- **Per window**: ~0.5 ms (2000 Hz EMG)
- **Real-time throughput**: 2000 samples/sec (1x real-time)
- **Latency**: ~250 ms (full pipeline with preprocessing)

### CPU Only Performance
- **Intel i7-10700 @ 3.8GHz**
- Per window: ~5-10 ms
- Real-time capable at ~200 Hz

## 🎓 Model Architecture

**RoFormer (Rotary Position Embedding Transformer)**
- Sequence-to-sequence prediction
- Input: 400 EMG samples (12 channels)
- Output: 10 joint angles
- Attention layers with rotary embeddings
- Pre-trained on Ninapro dataset

For architecture details, see `RoFormer.py`

## 📚 Additional Resources

- [PyTorch Documentation](https://pytorch.org/docs/stable/index.html)
- [Ninapro Dataset](https://ninapro.iit.unimi.it/)
- [Transformer Architecture Paper](https://arxiv.org/abs/1706.03762)
- [Rotary Position Embeddings](https://arxiv.org/abs/2104.09864)

## 🐛 Reporting Issues

When reporting issues, include:
1. OS and Python version
2. PyTorch version and GPU info (if applicable)
3. Error message and stack trace
4. Steps to reproduce
5. Your config.py settings (sanitized)

## 📄 License

See [LICENSE](LICENSE) file

## 🙏 Acknowledgments

- Ninapro dataset for EMG training data
- PyTorch team for the deep learning framework
- Unity team for the visualization engine

---
