# 🚀 Quick Start Guide

Get the sEMG-to-Hand system up and running in 10 minutes!

## 📋 Prerequisites

- **Windows, macOS, or Linux**
- **Python 3.8+** (for signal processing)
- **Unity 2022 LTS** (for visualization)
- **Internet connection** (to download dependencies)

## ⚡ 5-Minute Setup

### 1. Python Pipeline Setup (3 minutes)

```bash
# Navigate to Python project
cd RoFormer_Communication_Pipeline

# Create virtual environment
python -m venv venv

# Activate it
# On Windows:
venv\Scripts\activate
# On macOS/Linux:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt
```

**Done!** Python environment is ready.

### 2. Unity Project Setup (2 minutes)

1. Download and install [Unity Hub](https://unity.com/download)
2. Install Unity 2022 LTS (if not already installed)
3. In Unity Hub: **Add Project** → Select `FYP_Hand_Viz` folder
4. Wait for project to load (~1-2 minutes)
5. Open scene: `Assets/Scenes/HandVisualization.unity`

**Done!** Unity is ready.

## ▶️ Running the System

### Step 1: Start the Python Server

```bash
cd RoFormer_Communication_Pipeline
python gui.py
```

You should see:
```
✓ Model loaded successfully
✓ Server listening on 127.0.0.1:8051
✓ GUI window opened
```

### Step 2: Start Unity

1. In Unity Editor, click the **Play** button (▶️)
2. You should see the hand model in the scene
3. Check the Console (Ctrl+Shift+C) for messages

### Step 3: Test the Connection

**In Python GUI:**
- Load a sample EMG file (or let it generate test data)
- Click "Start Inference"
- Joint angles should appear in the console

**In Unity:**
- The hand should start moving
- Watch the Console for angle values

**Success! 🎉** Both systems are communicating!

## 🔧 Configuration (If Needed)

### Ports Don't Match?

**In Python** (`RoFormer_Communication_Pipeline/config.py`):
```python
LOCAL_PORT = 5005  # Change to your port
```

**In Unity** (`FYP_Hand_Viz/Assets/Scripts/JointReceiver.cs`):
```csharp
const int UDP_PORT = 5005;  // Change to match Python
```

### Running on Different Machines?

**On Python machine** (`config.py`):
```python
LOCAL_HOST = "0.0.0.0"  # Accept from any IP
```

**On Unity machine** (`JointReceiver.cs`):
```csharp
const string LOCAL_HOST = "192.168.x.x";  // IP of Python machine
```

## 📊 Test with Sample Data

### Use Pre-recorded Angles

**Python side:**
```bash
python pipeline.py --use-simulated-data
```

**Unity side:**
- Hand should animate smoothly

### Use Your Own EMG Data

```bash
python gui.py --mat-path "path/to/your/data.mat"
```

See [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md#-data-format) for data format.

## ⚠️ Common Issues & Quick Fixes

### "ModuleNotFoundError" (Python)
```bash
# Make sure virtual environment is ACTIVATED
# Windows:
venv\Scripts\activate
# macOS/Linux:
source venv/bin/activate

# Then install again
pip install -r requirements.txt
```

### "Port 8051 already in use"
```bash
# Find process using port 8051:
# Windows (PowerShell):
netstat -ano | Select-String "8051"

# Kill process (replace XXXX with PID):
taskkill /PID XXXX /F
```

### Hand not moving in Unity
1. Check Python console for errors
2. Verify port 8051 is listening: `netstat -an | grep 8051`
3. Check Unity Console (Ctrl+Shift+C) for network errors
4. Ensure firewall allows UDP on port 8051

### "Cannot find roformer.pt"
```bash
# Download from GitHub Releases and place in:
RoFormer_Communication_Pipeline/roformer.pt
```

## 📚 Next Steps

### To Use Real EMG Sensor
1. See [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md#-configuration)
2. Configure serial port or FPGA settings in `config.py`

### To Customize Hand Model
1. See [FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md#-common-adjustments-for-custom-hand-models)
2. Edit `JointReceiver.cs` to match your skeleton

### To Deploy on Network
1. Set Python to listen on all IPs
2. Obtain the Python machine's IP address
3. Update Unity to connect to that IP

## 📖 Full Documentation

- **[Python Pipeline](RoFormer_Communication_Pipeline/README.md)** - Complete signal processing guide
- **[Unity Project](FYP_Hand_Viz/README.md)** - Visualization and customization guide
- **[Main README](README.md)** - System architecture and integration

## 💡 Tips

- **Start Python first**, then Unity (connection initializes in this order)
- **Watch the Console output** for helpful debug information
- **Check firewall settings** if connection fails
- **Increase RAM allocation** in Unity Preferences if stuttering occurs

## 🎓 Understanding the Data Flow

```
1. Python loads EMG data (or reads from sensor)
   ↓
2. Preprocesses: filters, segments into windows (400 samples)
   ↓
3. Runs RoFormer model: EMG window → 10 joint angles
   ↓
4. Applies Kalman smoothing: noise reduction
   ↓
5. Sends via UDP (50-100 times per second)
   ↓
6. Unity receives angles on port 8051
   ↓
7. Updates hand joint rotations
   ↓
8. Renders 3D hand in real-time
```

## ✅ Checklist for Full Setup

- [ ] Python 3.8+ installed
- [ ] Virtual environment created and activated
- [ ] Python dependencies installed (`pip install -r requirements.txt`)
- [ ] `roformer.pt` model file downloaded
- [ ] Unity 2022 LTS installed
- [ ] Project loaded in Unity
- [ ] Scene opened (HandVisualization.unity)
- [ ] Python GUI running without errors
- [ ] Unity Play mode shows hand model
- [ ] Console shows connection messages
- [ ] Hand moves when Python sends angles

## 🆘 Still Having Issues?

1. **Read the full README files** in each project folder
2. **Check the Troubleshooting section** in the relevant README
3. **Verify all prerequisites** are installed correctly
4. **Check Console output** for detailed error messages
5. **Test components separately** (Python pipeline first, then Unity)

## 🎯 What to Try Next

Once everything is working:

1. **Load your own EMG data** - See data format in RoFormer README
2. **Tune Kalman filter** - Adjust `KALMAN_Q` and `KALMAN_R` in config.py
3. **Customize hand model** - Edit JointReceiver.cs for your skeleton
4. **Build for distribution** - File → Build Settings in Unity
5. **Deploy on FPGA** - Set `USE_FPGA=true` in config.py

---

**Need help?** Check the full documentation in the README files of each project folder.

**Last Updated**: June 2024
