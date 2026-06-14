# Quick Start Guide

## Prerequisites

- Python 3.8+
- pip or conda

## Setup Steps

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/RoFormer_Communication_Pipeline.git
cd RoFormer_Communication_Pipeline
```

### 2. Create Virtual Environment (Recommended)

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

### 3. Install Dependencies

```bash
pip install -r requirements.txt
```

### 4. Download the Model

The pre-trained model (`roformer.pt`) is required to run the pipeline. Download it from:
- GitHub Releases: [Add download link]
- Google Drive: [Add download link]

Place the downloaded file in the project root directory:
```
RoFormer_Communication_Pipeline/
├── roformer.pt  ← Place here
├── gui.py
├── pipeline.py
└── ...
```

### 5. Prepare Your Data

Place your EMG data in MAT format in the project directory or provide the path when running.

Expected MAT file structure:
```
{
    'emg': array([time_samples, 12 channels]),
    'glove': array([time_samples, 10 joint angles])
}
```

## Running the Application

### Option 1: GUI Application (Recommended)

```bash
python gui.py
```

Then:
1. Click "Load Data" and select your MAT file
2. Configure settings (Kalman filter, network, etc.)
3. Click "Start Server" and "Run Inference"
4. View real-time results

### Option 2: Command-Line Pipeline

Edit `pipeline.py` to set:
```python
MAT_PATH = "path/to/your/data.mat"
USE_FPGA = False  # True for FPGA, False for local
```

Then run:
```bash
python pipeline.py
```

### Option 3: Using Configuration File

Instead of hardcoding paths, use environment variables:

**Windows (PowerShell):**
```powershell
$env:ROFORMER_MAT_PATH = "C:\path\to\data.mat"
$env:ROFORMER_USE_FPGA = "false"
python gui.py
```

**Windows (Command Prompt):**
```cmd
set ROFORMER_MAT_PATH=C:\path\to\data.mat
set ROFORMER_USE_FPGA=false
python gui.py
```

**macOS/Linux:**
```bash
export ROFORMER_MAT_PATH="/path/to/data.mat"
export ROFORMER_USE_FPGA="false"
python gui.py
```

## Configuration

All settings are centralized in `config.py`. You can modify:

- **EMG Processing**: Window size, stride, filter settings
- **Kalman Filter**: Process noise (Q) and measurement noise (R)
- **Network**: Host, port, FPGA IP
- **Device**: GPU or CPU inference

See `config.py` for all available options.

## Troubleshooting

### Issue: "ModuleNotFoundError: No module named 'torch'"

**Solution:** Install PyTorch
```bash
pip install torch
```

For GPU support:
```bash
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

### Issue: "Model checkpoint not found"

**Solution:** Download `roformer.pt` and place it in the project root

### Issue: "FPGA connection failed"

**Solution:** 
1. Check if FPGA is online: `ping 192.168.1.10`
2. Verify IP in `config.py`: `FPGA_IP`
3. Make sure UDP port 5005 is not blocked

### Issue: "Serial port error" (COM4 not found)

**Solution:**
1. List available ports:
   ```bash
   python -c "import serial; print(serial.tools.list_ports.comports())"
   ```
2. Update `config.py` with correct port
3. Check device manager for COM port number

### Issue: Slow inference / High latency

**Solution:**
1. Use GPU if available (automatically detected)
2. Reduce window size in `config.py` (trade-off: lower accuracy)
3. Check network latency to FPGA

## Next Steps

- Read [README.md](README.md) for full documentation
- Check [RoFormer.py](RoFormer.py) for model architecture details
- Explore [load_dataset.py](load_dataset.py) for data preprocessing options

## Support

For issues:
1. Check the Troubleshooting section above
2. Review the [README.md](README.md) documentation
3. Open an issue on GitHub with:
   - Error message
   - Python version
   - Steps to reproduce

Happy inferencing! 🎉
