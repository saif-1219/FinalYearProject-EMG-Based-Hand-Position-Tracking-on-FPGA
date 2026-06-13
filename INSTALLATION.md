# Complete Installation Guide

Detailed step-by-step installation instructions for all platforms.

## Table of Contents
1. [Windows](#windows)
2. [macOS](#macos)
3. [Linux](#linux)
4. [Troubleshooting](#troubleshooting)

---

## Windows

### Prerequisites

#### Install Python 3.8 or Higher
1. Visit [python.org](https://www.python.org/downloads/)
2. Download Python 3.11 (or latest 3.8+)
3. **Important**: Check "Add Python to PATH" during installation
4. Verify installation:
   ```cmd
   python --version
   pip --version
   ```

#### Install Unity 2022 LTS
1. Download [Unity Hub](https://unity.com/download)
2. Install Unity Hub
3. In Hub: **Install** → Search for "2022" → Select **2022 LTS**
   - Include: "Windows Build Support (IL2CPP)"
4. Verify: Open Unity Hub and confirm 2022 LTS appears

#### Install Git (Optional but recommended)
1. Download [Git for Windows](https://git-scm.com/download/win)
2. Run installer with default settings
3. Verify:
   ```cmd
   git --version
   ```

### Python Pipeline Installation

#### Step 1: Open Command Prompt
- Press `Win + R`
- Type `cmd` and press Enter

#### Step 2: Navigate to Project
```cmd
cd "D:\Uni files\Semester-7\Capstone\My project (1)\fyp\RoFormer_Communication_Pipeline"
```

#### Step 3: Create Virtual Environment
```cmd
python -m venv venv
venv\Scripts\activate
```

You should see `(venv)` at the start of your command prompt.

#### Step 4: Install Dependencies
```cmd
pip install --upgrade pip
pip install -r requirements.txt
```

**This will take 5-10 minutes.** You'll see lots of download messages.

#### Step 5: Verify Installation
```cmd
python -c "import torch; print('PyTorch version:', torch.__version__)"
python -c "import tkinter; print('Tkinter: OK')"
```

### Unity Project Installation

#### Step 1: Open Unity Hub
- Click "Add Project"
- Browse to: `D:\Uni files\Semester-7\Capstone\My project (1)\fyp\FYP_Hand_Viz`
- Click "Open"

#### Step 2: Wait for Project to Load
- Unity will create Library folder (1-2 GB)
- Scripts will compile
- This takes 2-5 minutes on first load

#### Step 3: Open Main Scene
- In Project window: Assets → Scenes
- Double-click `HandVisualization.unity`
- Scene should load without errors

### Test Your Installation

#### Terminal 1: Start Python Server
```cmd
cd RoFormer_Communication_Pipeline
venv\Scripts\activate
python gui.py
```

Look for:
```
✓ Model loaded successfully
✓ Server listening on 127.0.0.1:8051
✓ GUI window opened
```

#### Terminal 2: Start Unity
- In Unity Editor, click the **Play** button (▶️)
- Check Console for connection messages

### Windows-Specific Issues

#### "Python command not found"
- **Solution**: Python not in PATH
- Reinstall Python and **check "Add Python to PATH"**

#### "pip: command not found"
- **Solution**: Use `python -m pip` instead
- Example: `python -m pip install torch`

#### "ModuleNotFoundError" in Python
- **Solution**: Ensure virtual environment is activated
  - Should see `(venv)` in command prompt
  - If not: `venv\Scripts\activate`

#### Port 8051 already in use
- **Solution**: Find and kill process
  ```cmd
  netstat -ano | findstr 8051
  taskkill /PID XXXX /F
  ```
  Replace `XXXX` with the PID number

---

## macOS

### Prerequisites

#### Install Homebrew
Open Terminal and run:
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

#### Install Python 3.8+
```bash
brew install python@3.11
brew link python@3.11
python3 --version  # Should show 3.11+
```

#### Install Xcode Command Line Tools (if needed)
```bash
xcode-select --install
```

#### Install Unity 2022 LTS
1. Download [Unity Hub](https://unity.com/download)
2. Open the .dmg file and drag to Applications
3. Open Unity Hub
4. Install → 2022 LTS
   - Include: "Mac Build Support"

#### Install Git (Optional)
```bash
brew install git
```

### Python Pipeline Installation

#### Step 1: Open Terminal
- Command + Space
- Type `Terminal`
- Press Enter

#### Step 2: Navigate to Project
```bash
cd ~/path/to/RoFormer_Communication_Pipeline
# Or use:
cd "/Volumes/[Drive]/Uni files/Semester-7/Capstone/My project (1)/fyp/RoFormer_Communication_Pipeline"
```

#### Step 3: Create Virtual Environment
```bash
python3 -m venv venv
source venv/bin/activate
```

You should see `(venv)` at the start of your prompt.

#### Step 4: Install Dependencies
```bash
pip install --upgrade pip
pip install -r requirements.txt
```

#### Step 5: Verify Installation
```bash
python -c "import torch; print('PyTorch version:', torch.__version__)"
python -c "import tkinter; print('Tkinter: OK')"
```

### Unity Project Installation

#### Step 1: Open Unity Hub
- Applications → Unity Hub
- Click "Add Project"
- Navigate to: `FYP_Hand_Viz` folder
- Click "Open"

#### Step 2: Wait for Import
- First load creates Library folder (~1-2 GB)
- Scripts compile
- 2-5 minutes typical

#### Step 3: Open Scene
- In Project: Assets → Scenes
- Double-click `HandVisualization.unity`

### Test Your Installation

#### Terminal 1: Python Server
```bash
cd ~/path/to/RoFormer_Communication_Pipeline
source venv/bin/activate
python gui.py
```

#### Terminal 2: Unity
- Click Play button in Unity Editor

### macOS-Specific Issues

#### "command not found: python3"
- **Solution**: Install via Homebrew
  ```bash
  brew install python@3.11
  brew link python@3.11
  ```

#### "Permission denied" on scripts
- **Solution**: Make executable
  ```bash
  chmod +x script_name.sh
  ```

#### Tkinter not found
- **Solution**: Reinstall Python with tkinter
  ```bash
  brew reinstall python@3.11
  ```

#### Port 8051 already in use
- **Solution**: Find and kill process
  ```bash
  lsof -i :8051
  kill -9 PID
  ```

---

## Linux

### Prerequisites (Ubuntu/Debian)

#### Install Python and Dependencies
```bash
sudo apt update
sudo apt install -y \
    python3.11 \
    python3-pip \
    python3-venv \
    python3-tk \
    git \
    curl
```

#### Install Unity
1. Download from [Unity Download](https://unity.com/download)
2. Or use:
   ```bash
   wget https://unity.com/download/download_unity
   chmod +x UnitySetup-2022.3.0f1
   ./UnitySetup-2022.3.0f1
   ```

### Python Pipeline Installation

#### Step 1: Open Terminal
- Press Ctrl + Alt + T

#### Step 2: Navigate to Project
```bash
cd ~/path/to/RoFormer_Communication_Pipeline
```

#### Step 3: Create Virtual Environment
```bash
python3 -m venv venv
source venv/bin/activate
```

You should see `(venv)` in your prompt.

#### Step 4: Install Dependencies
```bash
pip install --upgrade pip setuptools wheel
pip install -r requirements.txt
```

#### Step 5: Verify Installation
```bash
python -c "import torch; print('PyTorch version:', torch.__version__)"
python -c "import tkinter; print('Tkinter: OK')"
```

### Unity Project Installation

#### Step 1: Open Unity Hub
- Search for "Unity" in applications
- Click "Add Project"
- Navigate to FYP_Hand_Viz folder
- Click "Open"

#### Step 2: Wait for Import
- First load takes 2-5 minutes
- Library folder created (~1-2 GB)

#### Step 3: Open Scene
- Assets → Scenes → HandVisualization.unity

### Test Your Installation

#### Terminal 1: Python Server
```bash
cd ~/path/to/RoFormer_Communication_Pipeline
source venv/bin/activate
python gui.py
```

#### Terminal 2: Unity
- Click Play button in Unity Editor

### Linux-Specific Issues

#### "No module named 'tkinter'"
```bash
# Ubuntu/Debian
sudo apt install python3-tk

# Fedora
sudo dnf install python3-tkinter

# Arch
sudo pacman -S tk
```

#### "CUDA not found"
- **Solution**: Install NVIDIA driver
  ```bash
  sudo apt install nvidia-driver-520  # Adjust version
  nvidia-smi  # Verify installation
  ```

#### Port 8051 already in use
```bash
sudo lsof -i :8051
sudo kill -9 PID
```

---

## Troubleshooting

### General Issues

#### "ModuleNotFoundError: No module named 'X'"

**Solution**: Make sure virtual environment is activated
```bash
# Windows
venv\Scripts\activate

# macOS/Linux
source venv/bin/activate
```

Then reinstall:
```bash
pip install -r requirements.txt
```

#### "roformer.pt not found"
1. Download from GitHub Releases
2. Place in: `RoFormer_Communication_Pipeline/roformer.pt`
3. Verify file exists:
   ```bash
   ls -la roformer.pt  # macOS/Linux
   dir roformer.pt     # Windows
   ```

#### Network Connection Issues

**Test if port is listening:**
```bash
# macOS/Linux
netstat -an | grep 8051
lsof -i :8051

# Windows
netstat -ano | findstr "8051"
```

**Check firewall:**
- Windows: Windows Defender → Firewall → Allow app
- macOS: System Preferences → Security → Firewall Options
- Linux: `sudo ufw allow 8051/udp`

### GPU Installation

#### For NVIDIA GPU (CUDA)
```bash
# Install CUDA driver
# Windows: nvidia.com/Download/driverDetails.aspx
# macOS/Linux: Package manager

# Install CUDA toolkit
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

**Verify GPU:**
```bash
python -c "import torch; print('CUDA available:', torch.cuda.is_available())"
```

#### For AMD GPU
```bash
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/rocm5.7
```

#### Force CPU mode (if GPU issues)
```bash
export ROFORMER_DEVICE="cpu"  # macOS/Linux
set ROFORMER_DEVICE=cpu       # Windows
```

### Still Having Issues?

1. **Check System Requirements**
   - Python 3.8+: `python --version`
   - Disk space: 10+ GB free
   - RAM: 4GB minimum

2. **Update All Tools**
   ```bash
   pip install --upgrade pip setuptools wheel
   ```

3. **Reinstall Virtual Environment**
   ```bash
   # Remove old environment
   rm -rf venv  # macOS/Linux
   # rmdir /s /q venv  # Windows

   # Create new one
   python3 -m venv venv
   source venv/bin/activate  # macOS/Linux
   # venv\Scripts\activate  # Windows
   
   pip install -r requirements.txt
   ```

4. **Check Logs**
   - Python console output for error messages
   - Unity Console for script errors
   - System event logs for network issues

5. **Ask for Help**
   - Open GitHub Issue with error message
   - Include OS, Python version, traceback
   - Share configuration (sanitized)

---

## Next Steps

Once installation is complete:

1. **Read the Quick Start Guide**: [QUICKSTART.md](QUICKSTART.md)
2. **Follow the Main README**: [README.md](README.md)
3. **Check Project-Specific Docs**:
   - Python: [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md)
   - Unity: [FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md)

---

