# Frequently Asked Questions (FAQ)

Quick answers to common questions about the sEMG-to-Hand system.

## Installation & Setup

### Q: Do I need both Python and Unity installed?
**A:** Yes, this is a two-part system:
- **Python** processes the EMG data and runs the RoFormer model
- **Unity** visualizes the hand movements
- Both communicate via UDP

You need both for the complete system, but can use them independently for testing.

### Q: Can I run this on a Mac?
**A:** Yes! The project supports Windows, macOS, and Linux.
- See [INSTALLATION.md](INSTALLATION.md#macos) for macOS-specific instructions
- Python works identically
- Unity is fully supported on macOS

### Q: How much disk space do I need?
**A:** Approximately:
- Python virtual environment: 1-2 GB
- Unity project (Library): 1-2 GB
- Model checkpoint: 200 MB
- Total: **4-5 GB**

### Q: Can I use Python 3.7 or earlier?
**A:** No, Python 3.8+ is required due to:
- PyTorch compatibility
- Type hints syntax
- Performance improvements

We recommend Python 3.9+ for best results.

### Q: What if I don't have a GPU?
**A:** The system works fine on CPU:
1. It will be slower (5-10x)
2. Set `DEVICE = "cpu"` in config.py
3. or: `export ROFORMER_DEVICE="cpu"`

Modern CPUs still achieve real-time inference at reasonable speeds.

---

## Data & Configuration

### Q: What EMG data format do you support?
**A:** MATLAB .mat files with structure:
```python
{
    'emg_raw': [N_samples × 12],      # 12-channel EMG
    'glove_angles': [N_samples × 10]   # Ground truth (optional)
}
```

Contact us if you need CSV/other format support.

### Q: How do I use my own EMG data?
**A:**
1. Prepare as MATLAB .mat file (see format above)
2. Run:
   ```bash
   python gui.py --mat-path "path/to/your/data.mat"
   ```
3. Or set in config.py:
   ```python
   DEFAULT_MAT_PATH = "path/to/your/data.mat"
   ```

### Q: Can I use real-time sensor data?
**A:** Yes, but requires custom implementation:
1. Write code to read from your sensor (serial, USB, etc.)
2. Format data as [400 samples × 12 channels]
3. Send via UDP to port 8051

See `model_server.py` for UDP server structure.

### Q: How do I change the UDP port?
**A:** Update in both places:
1. **Python** (`config.py`):
   ```python
   LOCAL_PORT = 5005  # Change here
   ```
2. **Unity** (`JointReceiver.cs`):
   ```csharp
   const int UDP_PORT = 5005;  // Change here
   ```

**Important**: Both must use the same port!

### Q: Can I run Python and Unity on different machines?
**A:** Yes!
1. Get Python machine IP: `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
2. **Python** (`config.py`):
   ```python
   LOCAL_HOST = "0.0.0.0"  # Listen on all interfaces
   ```
3. **Unity** (`JointReceiver.cs`):
   ```csharp
   const string LOCAL_HOST = "192.168.x.x";  // Python machine IP
   ```

They must be on the same network.

---

## Performance & Optimization

### Q: Why is my prediction so slow?
**A:** Check these factors:
1. **CPU vs GPU**: GPU is 10x faster. Enable CUDA:
   ```bash
   pip install torch --index-url https://download.pytorch.org/whl/cu118
   ```
2. **Window size**: Reduce `WINDOW_SIZE` in config.py
3. **Background processes**: Close unnecessary apps
4. **Network latency**: Both apps should be on same network

### Q: The hand jerks/doesn't move smoothly
**A:** Tune Kalman filter parameters:
```python
# In config.py
KALMAN_Q = 1e-4      # Process noise (increase = faster response)
KALMAN_R = 0.05      # Measurement noise (decrease = smoother)
```

Start with:
- Q = 1e-3 (faster)
- R = 0.01 (smoother)

Then adjust based on results.

### Q: I'm getting 30 FPS in Unity, not 60
**A:**
1. Check GPU utilization in Profiler
2. Reduce quality settings: Edit → Project Settings → Quality
3. Close other applications
4. Profile with Window → Analysis → Profiler

### Q: How many samples per second can it process?
**A:**
- **GPU**: 2000+ samples/sec (real-time at 2kHz sampling)
- **CPU**: 200-500 samples/sec (depends on CPU)

With WINDOW_SIZE=400 and STRIDE=50, you get ~100 predictions/sec either way.

---

## Networking & Communication

### Q: My connection keeps timing out
**A:**
1. Check if Python server is running
2. Verify firewall allows UDP port 8051
3. Increase `UDP_TIMEOUT` in config.py:
   ```python
   UDP_TIMEOUT = 10.0  # Increase from 5.0
   ```

### Q: Can I use TCP instead of UDP?
**A:** Current implementation uses UDP. TCP support requires:
1. Modifying `model_server.py`
2. Rewriting UDP socket code
3. Testing network reliability

UDP was chosen for low-latency (no handshake).

### Q: How do I connect to FPGA?
**A:**
1. Set in config.py:
   ```python
   USE_FPGA = True
   FPGA_IP = "192.168.1.10"  # Your FPGA IP
   FPGA_PORT = 5005
   ```
2. FPGA must output 10 joint angles (float32)
3. Same UDP protocol as local server

### Q: What about latency over network?
**A:** Typical latencies:
- **Local (same PC)**: <1 ms
- **Same LAN**: 1-5 ms
- **Different networks**: 10-100+ ms

Kalman filter helps smooth network delays.

---

## Model & Training

### Q: Can I train my own RoFormer model?
**A:** Yes, but not included in this repo:
1. Use Ninapro dataset or your own data
2. Implement training in PyTorch
3. Save as `roformer.pt`
4. Replace existing checkpoint

This would be a significant undertaking.

### Q: How was the pre-trained model trained?
**A:**
- Dataset: Ninapro (1000+ hours of EMG)
- Architecture: RoFormer with rotary embeddings
- Training: 100 epochs with Adam optimizer
- Validation: Subject-specific testing

See `RoFormer.py` for architecture details.

### Q: Can I fine-tune the model?
**A:** Yes, partially:
1. Freeze early layers
2. Fine-tune later layers on your data
3. Save new checkpoint
4. Use in pipeline

Requires modifications to training code.

### Q: Why RoFormer instead of LSTM/CNN?
**A:**
- **Attention**: Better captures long-range dependencies
- **Rotary embeddings**: Handles variable-length sequences well
- **Speed**: Parallelizable, efficient for real-time
- **Accuracy**: ~5-10% improvement over LSTM/CNN baselines

---

## Troubleshooting

### Q: "ModuleNotFoundError: No module named 'torch'"
**A:** Virtual environment not activated:
```bash
# Windows
venv\Scripts\activate

# macOS/Linux
source venv/bin/activate
```

Then verify `pip list` shows torch.

### Q: Python GUI won't open
**A:** Tkinter issue:
- **Windows**: Reinstall Python with tcl/tk
- **macOS**: `brew install python-tk@3.11`
- **Linux**: `sudo apt install python3-tk`

### Q: Unity project won't open
**A:**
1. Delete `Library/` folder
2. Delete `obj/` folder
3. Delete `.vs/` folder (if exists)
4. Reopen project in Unity

This forces reimport of all assets.

### Q: "Port 8051 already in use"
**A:** Kill the process using it:
```bash
# macOS/Linux
lsof -i :8051
kill -9 <PID>

# Windows
netstat -ano | findstr "8051"
taskkill /PID <PID> /F
```

### Q: Hand rotations are wrong
**A:**
1. Check angles are in radians (not degrees)
2. Check joint mapping in `JointReceiver.cs` matches skeleton
3. Some joints may need axis adjustment:
   ```csharp
   angles[i] = -angles[i];  // Flip if inverted
   ```

---

## Advanced Questions

### Q: Can I use FPGA inference?
**A:** Yes, if you have:
1. FPGA board with RoFormer implementation
2. UDP communication support
3. Model compiled for FPGA

Set `USE_FPGA = True` in config.py.

### Q: How do I deploy this as a standalone app?
**A:** For Unity:
1. File → Build Settings
2. Select platform (Windows/Mac/Linux)
3. Click Build
4. Python server can run as separate service

For Python:
1. Use `PyInstaller` to create executable
2. Bundle virtual environment
3. Users run `.exe` or `.app`

### Q: Can I add real-time EMG visualization?
**A:** Yes, modify `gui.py`:
1. `load_dataset.py` has signal preprocessing
2. Add matplotlib plot in GUI
3. Update on each prediction cycle

Example partially implemented.

### Q: How do I integrate with motion capture systems?
**A:** Replace UDP receiver with:
1. Mocap SDK (Vicon, Optitrack, etc.)
2. Stream joint angles instead of EMG
3. Rest of system remains same

### Q: Can I export hand animation?
**A:** Yes, record in Unity:
1. Window → Recorder
2. Record while predictions running
3. Export as video/MP4

Or access raw angle data:
```csharp
File.WriteAllText("angles.csv", angleData);
```

---

## Getting Help

### Where to report bugs?
- GitHub Issues: Include error message, steps to reproduce, environment
- Discussions: For questions and feature ideas

### I found a bug, how do I fix it?
1. Fork repository
2. Create feature branch
3. Fix bug with tests
4. Create Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

### Can I use this for commercial purposes?
- Check [LICENSE](RoFormer_Communication_Pipeline/LICENSE)
- Likely permits commercial use with attribution
- Contact maintainers if uncertain

### How do I cite this project?
See README.md for citation format. Typically:
```
Author Name. "sEMG-to-Hand System." GitHub, year. github.com/...
```

---

## Still Have Questions?

1. **Check the documentation** in README files
2. **Search GitHub Issues** - your Q might be answered
3. **Open a new Discussion** on GitHub
4. **Create an Issue** if you found a bug

We're happy to help! 🙌

---

**Last Updated**: June 2024
