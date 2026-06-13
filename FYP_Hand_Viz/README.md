# FYP Hand Visualization - Unity Application

Real-time 3D hand gesture visualization with joint angle prediction from EMG signals. This Unity project receives hand joint angle data via UDP and visualizes it on a high-fidelity 3D hand model.

## 📋 Project Overview

This is the visualization component of the sEMG-to-Hand system. It works in tandem with the **RoFormer Communication Pipeline** to:
- ✅ Receive predicted joint angles via UDP
- ✅ Display real-time 3D hand animation
- ✅ Show predicted vs actual angles comparison
- ✅ Support multiple hand models (customizable skeleton)
- ✅ Provide interactive camera controls

## 🎯 System Architecture

```
RoFormer Pipeline (Python)
    ↓ UDP Port 8051 (Joint angles)
Unity Application
    ↓
3D Hand Visualization
    ↓
Real-time Display
```

## ✨ Features

- 🤚 **10 DOF Hand Model**: MCP + PIP joints for all fingers
- 📊 **Real-time Visualization**: Live updates at 50+ fps
- 🔴 **Predicted vs Actual**: Side-by-side comparison display
- 🎮 **Interactive Controls**: Rotate, zoom, and pan the hand
- 🔌 **UDP Communication**: Receive data from Python pipeline
- 📈 **Data Logging**: Save angle data to files for analysis
- 🎨 **Customizable**: Easy to adapt skeleton hierarchy

## 📁 Project Structure

```
FYP_Hand_Viz/
├── Assets/
│   ├── Scripts/
│   │   ├── JointReceiver.cs         # Main UDP receiver script
│   │   ├── TransformerModel.cs      # Model inference interface
│   │   └── MatlabHandSimulator.cs   # Data playback simulator
│   ├── Models/
│   │   └── [Hand model prefabs]
│   ├── Materials/
│   │   └── [Visual materials]
│   ├── Scenes/
│   │   └── HandVisualization.unity  # Main scene
│   ├── TextMesh Pro/
│   │   └── [UI fonts and resources]
│   └── XR/
│       └── [XR Hands sample package]
├── ProjectSettings/
│   └── [Unity project configuration]
├── Packages/
│   ├── manifest.json                # Package dependencies
│   └── packages-lock.json           # Locked versions
├── FYP_Hand_Viz.sln                 # Visual Studio solution file
└── README.md                        # This file
```

## 📋 Requirements

### System Requirements
| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Windows 10, macOS 10.13, Ubuntu 18.04 | Windows 11, macOS 12+, Ubuntu 20.04+ |
| Unity | 2022 LTS | 2022 LTS or 2023+ |
| RAM | 4GB | 8GB+ |
| GPU | Intel HD, AMD Radeon, NVIDIA GTX | NVIDIA RTX 20 series or better |
| .NET | 4.7.1 | 4.7.1+ |
| Network | UDP capable | Same LAN as Python server |

### Unity Packages
- **XR Hands**: Included in the project
- **TextMesh Pro**: Usually pre-installed
- **.NET Framework**: 4.7.1+

## 🚀 Getting Started

### Step 1: Install Unity

1. Download [Unity Hub](https://unity.com/download)
2. Install Unity 2022 LTS:
   - Unity Hub → Install → Select 2022 LTS version
   - Include: Windows Build Support (or Mac/Linux as needed)

### Step 2: Open the Project

```
1. Unity Hub → Add Project → Select FYP_Hand_Viz folder
2. Wait for project to load and compile scripts
3. Go to Assets/Scenes and open HandVisualization.unity
```

### Step 3: Configure Network Settings

In the Inspector, select the hand GameObject:

1. Find the **JointReceiver** script component
2. Set **UDP Port** to match Python pipeline:
   - Default: `8051`
   - Must match `LOCAL_PORT` in `../RoFormer_Communication_Pipeline/config.py`

### Step 4: Start Python Server

Before running Unity, ensure the Python pipeline is running:

```bash
cd ../RoFormer_Communication_Pipeline
python gui.py
```

or

```bash
python model_server.py
```

### Step 5: Run Unity Application

1. Press **Play** (▶ button) in Unity Editor
2. Hand model should start receiving and displaying angles
3. Check Console (Ctrl+Shift+C) for any connection errors

## 📖 Usage Guide

### Basic Workflow

1. **Start Python Pipeline**
   ```bash
   cd RoFormer_Communication_Pipeline
   python gui.py
   ```

2. **Open Unity Project**
   - Unity Hub → FYP_Hand_Viz → Open

3. **Configure (if needed)**
   - Select hand GameObject
   - Verify UDP port in JointReceiver script

4. **Play**
   - Press Play button or Ctrl+P
   - Monitor Console for connection status

5. **Interact**
   - Use mouse to rotate/zoom hand
   - Scroll wheel to zoom in/out
   - Right-click and drag to pan

### Script Components

#### JointReceiver.cs
Main script that:
- Listens for UDP packets on port 8051
- Parses float32 joint angle data
- Updates hand skeleton in real-time
- Logs angles to console for debugging

**Key Variables:**
```csharp
private Transform rIndexProximal;      // Index finger MCP joint
private Transform rIndexIntermediate;  // Index finger PIP joint
// ... (similar for other fingers and thumb)

void UpdateHandPoseFromAngles(float[] angles)
{
    // angles[0-1]: Thumb MCP/PIP
    // angles[2-3]: Index MCP/PIP
    // angles[4-5]: Middle MCP/PIP
    // angles[6-7]: Ring MCP/PIP
    // angles[8-9]: Pinky MCP/PIP
}
```

#### TransformerModel.cs
Interface for model inference (optional):
- Can be used to run inference locally in Unity
- Currently not active in the visualization scene

#### MatlabHandSimulator.cs
Playback for pre-recorded data:
- Load and replay angle data
- Useful for testing without Python server
- Format: text file with 10 floats per line

### Data Flow

```
Python UDP Stream (Port 8051)
    ↓ (10 float32 values = 40 bytes)
JointReceiver.cs listens on socket
    ↓
Parse angles from byte buffer
    ↓
Convert radians to degrees
    ↓
Update hand transforms
    ↓
Real-time 3D visualization
    ↓ (Optional) Log to file
```

## ⚙️ Configuration

### Network Settings

Edit **JointReceiver.cs**:
```csharp
const int UDP_PORT = 8051;  // Change if needed
const string LOCAL_HOST = "127.0.0.1";  // For local testing
```

For remote Python server:
```csharp
const string LOCAL_HOST = "192.168.x.x";  // IP of Python machine
```

### Hand Model Customization

If using a different skeleton hierarchy:

1. In JointReceiver.cs, modify the `CacheHandTransforms()` function
2. Update joint mappings in `UpdateHandPoseFromAngles()`
3. Match skeleton hierarchy to your hand model

Example for different finger structure:
```csharp
// If your hand model has different joint names:
rIndexProximal = transform.Find("Hand/Fingers/Index/MCP");  // Adjust path
rIndexIntermediate = transform.Find("Hand/Fingers/Index/PIP");
```

### UI Customization

To add angle display on screen:

1. Create Canvas → Text element
2. Create new script to subscribe to JointReceiver angles
3. Update text every frame with formatted angle values

Example:
```csharp
public class AngleDisplay : MonoBehaviour
{
    public Text angleText;
    private JointReceiver receiver;

    void Start()
    {
        receiver = FindObjectOfType<JointReceiver>();
    }

    void Update()
    {
        if (receiver != null && receiver.ActualJointAngles != null)
        {
            angleText.text = "Angles:\n" + 
                string.Join("\n", receiver.ActualJointAngles);
        }
    }
}
```

## 🔌 UDP Connection Details

### Port Configuration
- **Default Port**: 8051
- **Protocol**: UDP (connectionless)
- **Timeout**: None (waits indefinitely)

### Message Format

**Received Data:**
- 40 bytes (10 × 4-byte float32 values)
- Order: [Thumb MCP, Thumb PIP, Index MCP, Index PIP, Middle MCP, Middle PIP, Ring MCP, Ring PIP, Pinky MCP, Pinky PIP]
- Units: **Radians** (converted to degrees in script)
- Byte order: Little-endian (typical for x86)

### Example Python Sender
```python
import socket
import numpy as np

def send_angles(angles, port=8051):
    """Send 10 joint angles to Unity"""
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    data = np.array(angles, dtype=np.float32).tobytes()
    sock.sendto(data, ("127.0.0.1", port))
    sock.close()

# Send sample angles (radians)
angles = [0.1, 0.2, 0.15, 0.25, 0.12, 0.22, 0.1, 0.2, 0.08, 0.18]
send_angles(angles)
```

## 🐛 Troubleshooting

### Unity Project Won't Open

**Problem**: "Missing dependency" or can't find project
- **Solution 1**: Delete `Library/` folder and reimport
- **Solution 2**: Ensure Unity 2022 LTS is installed
- **Solution 3**: Right-click project → Generate .csproj files

```bash
# Terminal
cd FYP_Hand_Viz
rm -rf Library  # macOS/Linux
# or delete Library folder manually on Windows
```

### No Data Received

**Problem**: Hand doesn't move, console shows no angles

1. **Check Python Server**
   ```bash
   # In Python terminal
   python -c "import socket; s = socket.socket(); s.bind(('127.0.0.1', 8051))"
   # Should complete without error
   ```

2. **Verify Port in Unity**
   - Select hand GameObject → Inspector
   - Check JointReceiver script has UDP_PORT = 8051

3. **Check Firewall**
   - Windows: Allow UDP on port 8051
   - macOS: System Preferences → Security → Firewall Options

4. **Verify Network**
   ```bash
   # macOS/Linux
   netstat -an | grep 8051
   
   # Windows (PowerShell)
   netstat -an | Select-String "8051"
   ```

### Hand Rotations Look Wrong

**Problem**: Hand moves but rotations don't look realistic

1. **Check Joint Mapping**
   - Open JointReceiver.cs
   - Verify line 80-120 matches your skeleton hierarchy
   - Use Debug.Log to print angles and verify they're in correct order

2. **Check Angle Units**
   - Ensure angles are in radians from Python
   - Check conversion: `angle_degrees = angle_radians * 180 / π`

3. **Verify Skeleton Setup**
   - Hand model might have different joint orientations
   - May need to adjust axis or negate angles
   - Example: `angles[0] = -angles[0];` to flip rotation

### Poor Performance (Low FPS)

**Problem**: Frame rate drops below 30 fps

1. **Check GPU Utilization**
   - Window → Analysis → Profiler (Ctrl+7)
   - Look for CPU or GPU bottleneck

2. **Reduce Visual Quality**
   - Edit → Project Settings → Quality
   - Reduce shadow distance, particle count
   - Lower hand model polygon count if possible

3. **Check UDP Processing**
   - JointReceiver.cs Update() may be inefficient
   - Profile network socket operations
   - Consider receiving in background thread

### Console Errors

**"SocketException: Address already in use"**
```bash
# Port is already bound to another process
# Kill process on port 8051:
# Windows: netstat -ano | findstr 8051, then taskkill /PID xxx
# macOS/Linux: lsof -i :8051, then kill -9 <PID>
```

**"Failed to parse joint angles"**
- Check Python is sending data in correct format
- Verify network connectivity
- Check byte order matches (little-endian assumed)

**"NullReferenceException: Hand transforms not found"**
- Skeleton hierarchy doesn't match expectations
- Edit CacheHandTransforms() to use correct transform paths
- Use Transform.Find() with your actual hierarchy

## 📈 Performance Optimization

### Tips for Smooth Visualization

1. **Receive Data in Background Thread**
   ```csharp
   private Thread udpThread;
   
   void Start()
   {
       udpThread = new Thread(ReceiveUdpData) { IsBackground = true };
       udpThread.Start();
   }
   ```

2. **Queue Angle Updates**
   ```csharp
   private Queue<float[]> angleQueue = new Queue<float[]>();
   
   void Update()
   {
       if (angleQueue.Count > 0)
       {
           float[] angles = angleQueue.Dequeue();
           UpdateHandPose(angles);
       }
   }
   ```

3. **Reduce Update Frequency**
   - Only update on significant angle changes
   - Skip frames if receiving faster than 60 Hz
   - Consider receiving at 30-50 Hz for smooth visuals

### Network Optimization

- **Buffering**: Receive multiple packets before processing
- **Compression**: Could compress angle data (if needed)
- **Bandwidth**: Current setup uses ~2KB/s at 50 Hz

## 🎮 Build & Distribution

### Building for Windows

1. File → Build Settings
2. Select PC, Mac & Linux Standalone
3. Target Platform: Windows
4. Architecture: x86_64
5. Build

### Building for macOS

```bash
File → Build Settings
Select macOS
Architecture: Apple Silicon or Intel
Build
```

### Building for Linux

```bash
File → Build Settings
Select Linux
Architecture: x86_64
Build
```

## 📚 Additional Resources

- [Unity Manual](https://docs.unity3d.com/Manual/)
- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [UDP/Networking in Unity](https://docs.unity3d.com/ScriptReference/UdpClient.html)
- [Hand Animation Best Practices](https://docs.unity3d.com/Manual/avatar-HumanIK.html)

## ⚠️ Common Adjustments for Custom Hand Models

### For Hand Models with Different Skeleton

1. **Identify your joint structure**
   ```
   YourHand/
   ├── Thumb
   │   ├── ThumbMCP
   │   └── ThumbPIP
   ├── Index
   │   ├── IndexMCP
   │   └── IndexPIP
   └── ...
   ```

2. **Update CacheHandTransforms()**
   ```csharp
   void CacheHandTransforms()
   {
       rThumbProximal = transform.Find("YourHand/Thumb/ThumbMCP");
       rThumbDistal = transform.Find("YourHand/Thumb/ThumbPIP");
       // ... etc
   }
   ```

3. **Adjust rotation axes if needed**
   ```csharp
   // If joints rotate on different axis
   rIndexProximal.localRotation = Quaternion.Euler(0, angles[2], 0);  // Y-axis
   // instead of
   rIndexProximal.localRotation = Quaternion.Euler(angles[2], 0, 0);  // X-axis
   ```

## 🔄 Integration with Python Pipeline

### Full System Checklist

- [ ] Python pipeline installed and tested
- [ ] Model weights (`roformer.pt`) downloaded
- [ ] `config.py` settings verified (especially UDP port)
- [ ] Python GUI or server running
- [ ] Unity project opened without errors
- [ ] Network connectivity confirmed
- [ ] UDP firewall rules configured
- [ ] JointReceiver script attached to hand
- [ ] Port settings match (8051 default)

## 📄 License

See [LICENSE](../RoFormer_Communication_Pipeline/LICENSE)

## 🙏 Acknowledgments

- Unity XR Hands sample for hand tracking reference
- TextMesh Pro for UI rendering
- The open-source communities for various tools and libraries

---

