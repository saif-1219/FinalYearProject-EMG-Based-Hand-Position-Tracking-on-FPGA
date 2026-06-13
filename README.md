# sEMG-to-Hand Visualization System

A complete end-to-end system for real-time hand gesture recognition and visualization using Surface Electromyography (sEMG) signals. This repository contains two integrated applications: a Python-based signal processing pipeline with GUI, and a Unity 3D visualization application.

## 🎯 System Overview

```
┌─────────────────────┐
│  EMG Sensor Data    │
└──────────┬──────────┘
           │
           ↓
┌──────────────────────────────────────┐
│ RoFormer Communication Pipeline      │
│  • Preprocess EMG signals            │
│  • RoFormer Model Inference          │
│  • Kalman Filtering                  │
│  • UDP Transmission (Port 8051)      │
└──────────┬───────────────────────────┘
           │
           ↓ (UDP Packets)
┌──────────────────────────────────────┐
│ FYP Hand Visualization (Unity)       │
│  • Real-time 3D Hand Model           │
│  • Joint Angle Visualization         │
│  • Predicted vs Actual Comparison    │
└──────────────────────────────────────┘
```

## 📁 Project Structure

```
.
├── RoFormer_Communication_Pipeline/  ← Python signal processor & GUI
│   ├── gui.py                        # Main GUI application (tkinter)
│   ├── model_server.py               # Local model inference server
│   ├── pipeline.py                   # End-to-end processing pipeline
│   ├── RoFormer.py                   # Model architecture
│   ├── load_dataset.py               # Data loading & preprocessing
│   ├── Kalman.py                     # Kalman filtering
│   ├── config.py                     # Configuration settings
│   ├── requirements.txt              # Python dependencies
│   ├── roformer.pt                   # Pre-trained model weights
│   ├── README.md                     # Detailed setup guide
│   └── SETUP.md                      # Quick start guide
│
├── FYP_Hand_Viz/                     ← Unity 3D visualization
│   ├── Assets/
│   │   └── Scripts/
│   │       ├── JointReceiver.cs      # UDP receiver for joint angles
│   │       ├── TransformerModel.cs   # Model inference interface
│   │       └── MatlabHandSimulator.cs
│   ├── FYP_Hand_Viz.sln              # Unity project solution
│   ├── ProjectSettings/              # Unity configuration
│   ├── README.md                     # Setup guide
│   └── [Unity Asset directories]
│
└── README.md (this file)
```

## ✨ Features

### RoFormer Communication Pipeline
- 🎯 **RoFormer Model**: Transformer-based architecture with Rotary Position Embeddings
- 📊 **Real-time GUI**: Interactive tkinter application with live parameter adjustment
- 🔄 **Multiple Backends**: Local CPU/GPU inference or FPGA via UDP
- 🎚️ **Kalman Filtering**: Adaptive noise reduction for smooth predictions
- 🔌 **Flexible Communication**: UDP/UART support for different hardware setups
- 📈 **Live Visualization**: Real-time plots of EMG signals and joint angles

### FYP Hand Visualization
- 🤚 **3D Hand Model**: Real-time visualization using Unity's XR Hands
- 📍 **Joint Mapping**: Accurate 10 DOF hand model (MCP + PIP joints)
- 🔴 **Live Updates**: UDP-based real-time angle reception
- 📊 **Comparison Mode**: Side-by-side predicted vs actual angles
- 🎮 **Interactive**: Full 3D manipulation and visualization

## 🚀 Quick Start

### Prerequisites
- **For Python Pipeline**: Python 3.8+, pip
- **For Unity**: Unity 2022 LTS or later
- **For Communication**: Both on same network or localhost

### Option 1: Run Locally (Testing)

1. **Set up Python environment**
   ```bash
   cd RoFormer_Communication_Pipeline
   python -m venv venv
   # Windows
   venv\Scripts\activate
   # macOS/Linux
   source venv/bin/activate
   pip install -r requirements.txt
   ```

2. **Start the model server**
   ```bash
   python gui.py
   ```

3. **Open Unity project**
   - Unity Editor → File → Open Project
   - Select `FYP_Hand_Viz` folder
   - Press Play to start receiving data

### Option 2: Using Your Own EMG Data

See [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md) for detailed instructions on loading your own MAT files or connecting real sensors.

## 📋 System Requirements

### Python Pipeline
| Component | Requirement |
|-----------|------------|
| Python | 3.8+ |
| PyTorch | 2.0+ |
| NumPy | 1.21+ |
| SciPy | 1.7+ |
| RAM | 4GB minimum (8GB recommended) |
| GPU | Optional (CUDA 11.8+ for acceleration) |

### Unity Visualization
| Component | Requirement |
|-----------|------------|
| Unity | 2022 LTS or later |
| .NET | 4.7.1+ |
| XR Hands | Sample package (included) |
| RAM | 4GB minimum |
| GPU | Recommended (for smooth visualization) |

## 🔧 Configuration

Both applications use centralized configuration files:

- **Python**: See `RoFormer_Communication_Pipeline/config.py`
  - UDP port settings
  - EMG processing parameters
  - Kalman filter tuning
  - Model checkpoint path

- **Unity**: JointReceiver.cs has hardcoded settings
  - Default UDP port: 8051
  - Joint angle mapping

**Environment Variables** can override defaults:
```bash
export ROFORMER_CHECKPOINT="/path/to/model.pt"
export ROFORMER_MAT_PATH="/path/to/data.mat"
export ROFORMER_DEVICE="cuda"  # or "cpu"
export ROFORMER_FPGA_IP="192.168.1.10"
export ROFORMER_FPGA_PORT="5005"
```

## 📚 Documentation

- **[RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md)** - Complete Python pipeline documentation
- **[RoFormer_Communication_Pipeline/SETUP.md](RoFormer_Communication_Pipeline/SETUP.md)** - Quick start for Python pipeline
- **[FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md)** - Unity project setup guide

## ⚠️ Common Issues & Troubleshooting

### Python Pipeline

**Issue**: "ModuleNotFoundError: No module named 'torch'"
- **Solution**: Ensure virtual environment is activated and dependencies installed
  ```bash
  source venv/bin/activate  # or venv\Scripts\activate on Windows
  pip install -r requirements.txt
  ```

**Issue**: "Cannot load model checkpoint"
- **Solution**: Verify `roformer.pt` exists in the project root:
  ```bash
  ls -la roformer.pt  # macOS/Linux
  dir roformer.pt     # Windows
  ```
- Download from GitHub Releases if missing

**Issue**: "UDP connection refused" or timeout
- **Solution**: 
  - Check if Unity is running and listening on port 8051
  - Verify firewall settings (Windows Defender, macOS Firewall)
  - For network systems, ensure both machines are on the same network

### Unity Project

**Issue**: Not receiving data (blank hand visualization)
- **Solution**:
  - Verify JointReceiver.cs is attached to the hand GameObject
  - Check console for connection errors
  - Ensure Python pipeline is running and sending data
  - Verify UDP port matches in both applications

**Issue**: Hand rotations look incorrect
- **Solution**: The joint mapping may need adjustment based on your hand model
  - Edit JointReceiver.cs line ~80-120 to match your skeleton hierarchy
  - Use Debug.Log output to verify angle values

**Issue**: Unity won't open the project
- **Solution**:
  - Ensure Unity 2022 LTS is installed
  - Try: File → Open Project (not recent projects)
  - Delete Library/ folder and reimport

## 🔌 Connection Details

### UDP Communication Protocol

**Default Settings**:
- Host: `127.0.0.1` (localhost) or your machine IP
- Port: `8051`
- Protocol: UDP

**Data Format**:
- Input: 400 EMG samples (float32, ~2kHz sampling rate)
- Output: 10 joint angles (float32, radians)

### Message Flow
1. Python pipeline receives EMG data (raw or from file)
2. Processes and segments into 400-sample windows
3. Sends each window via UDP to port 8051
4. Receives 10 joint angles back from model
5. Applies Kalman smoothing
6. Unity receives and visualizes in real-time

## 📝 Model Training & Evaluation

The RoFormer model is pre-trained on sEMG data from the Ninapro dataset. For training details:
- Refer to `RoFormer.py` for architecture details
- See `load_dataset.py` for data format specifications
- Check `pipeline.py` for evaluation protocols

## 🎓 Academic Reference

This project is a capstone implementation for hand gesture recognition using sEMG signals.

**Model**: RoFormer (Rotary Position Embedding Transformer)
**Dataset**: Compatible with Ninapro format
**Application**: Real-time hand gesture visualization

## 🤝 Contributing

To improve or extend this project:
1. Fork the repository
2. Create a feature branch
3. Make your improvements
4. Submit a pull request

## 📄 License

See [LICENSE](RoFormer_Communication_Pipeline/LICENSE) file

## ✉️ Support & Contact

For issues, questions, or suggestions:
- Check troubleshooting sections above
- Review README files in individual project folders
- Check the config.py for parameter tuning options

---

