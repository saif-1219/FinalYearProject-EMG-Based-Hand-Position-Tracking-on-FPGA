# Documentation Index

Complete guide to all documentation files in the sEMG-to-Hand system.

## 📚 Core Documentation

### Start Here
- **[README.md](README.md)** - Main project overview and system architecture
- **[QUICKSTART.md](QUICKSTART.md)** - Get up and running in 10 minutes (⭐ START HERE!)
- **[INSTALLATION.md](INSTALLATION.md)** - Detailed OS-specific installation steps

### For Developers
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - How to contribute, coding standards, workflow
- **[FAQ.md](FAQ.md)** - Answers to common questions
- **[DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)** - This file

---

## 📁 Project-Specific Documentation

### RoFormer Communication Pipeline (Python)
**Location**: `RoFormer_Communication_Pipeline/`

| File | Purpose |
|------|---------|
| [README.md](RoFormer_Communication_Pipeline/README.md) | Complete pipeline documentation, features, API reference |
| [SETUP.md](RoFormer_Communication_Pipeline/SETUP.md) | Quick start guide for Python setup |
| [config.py](RoFormer_Communication_Pipeline/config.py) | Configuration file - **EDIT THIS for customization** |
| [requirements.txt](RoFormer_Communication_Pipeline/requirements.txt) | Python dependencies |

**Key Scripts:**
- `gui.py` - Interactive tkinter GUI (run this to start)
- `pipeline.py` - Command-line pipeline
- `model_server.py` - Standalone UDP server
- `RoFormer.py` - Model architecture and utilities
- `load_dataset.py` - Data loading and preprocessing
- `Kalman.py` - Kalman filter implementation

### FYP Hand Visualization (Unity)
**Location**: `FYP_Hand_Viz/`

| File | Purpose |
|------|---------|
| [README.md](FYP_Hand_Viz/README.md) | Unity project setup, customization, troubleshooting |
| [JointReceiver.cs](FYP_Hand_Viz/Assets/Scripts/JointReceiver.cs) | Main UDP receiver - **EDIT THIS for custom skeleton** |

**Project Files:**
- `FYP_Hand_Viz.sln` - Visual Studio solution
- `Assets/Scenes/HandVisualization.unity` - Main scene
- `ProjectSettings/` - Unity configuration

---

## 🎯 Quick Navigation

### I want to...

#### Get Started Quickly
👉 [QUICKSTART.md](QUICKSTART.md)

#### Install Everything Properly
👉 [INSTALLATION.md](INSTALLATION.md)

#### Understand the System
👉 [README.md](README.md)

#### Run the Python Pipeline
👉 [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md)

#### Set Up the Visualization
👉 [FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md)

#### Customize Configuration
👉 [RoFormer_Communication_Pipeline/config.py](RoFormer_Communication_Pipeline/config.py)

#### Contribute Code
👉 [CONTRIBUTING.md](CONTRIBUTING.md)

#### Get Answers to Common Questions
👉 [FAQ.md](FAQ.md)

#### Deploy on Different Machines
👉 [README.md - Configuration](README.md#-configuration)

#### Troubleshoot Issues
👉 [RoFormer_Communication_Pipeline/README.md - Troubleshooting](RoFormer_Communication_Pipeline/README.md#-troubleshooting)
👉 [FYP_Hand_Viz/README.md - Troubleshooting](FYP_Hand_Viz/README.md#-troubleshooting)

---

## 📖 Complete File Structure

```
sEMG-to-Hand/
│
├── README.md                    ← Start here for overview
├── QUICKSTART.md               ← 10-minute setup
├── INSTALLATION.md             ← Detailed per-OS instructions
├── CONTRIBUTING.md             ← How to contribute
├── FAQ.md                       ← Common questions
├── DOCUMENTATION_INDEX.md       ← This file
├── .gitignore                   ← Git configuration
│
├── RoFormer_Communication_Pipeline/
│   ├── README.md               ← Python project docs
│   ├── SETUP.md                ← Python quick setup
│   ├── config.py               ← Configuration (EDIT THIS!)
│   ├── requirements.txt        ← Python dependencies
│   ├── gui.py                  ← Main GUI application
│   ├── pipeline.py             ← Command-line pipeline
│   ├── model_server.py         ← UDP server
│   ├── RoFormer.py             ← Model architecture
│   ├── load_dataset.py         ← Data loading
│   ├── Kalman.py               ← Kalman filter
│   ├── roformer.pt             ← Model weights (download)
│   └── .gitignore
│
├── FYP_Hand_Viz/
│   ├── README.md               ← Unity project docs
│   ├── FYP_Hand_Viz.sln        ← Visual Studio solution
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── JointReceiver.cs        ← Main script (EDIT FOR CUSTOM SKELETON)
│   │   │   ├── TransformerModel.cs
│   │   │   └── MatlabHandSimulator.cs
│   │   ├── Scenes/
│   │   │   └── HandVisualization.unity ← Main scene
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── TextMesh Pro/
│   │   └── XR/
│   ├── ProjectSettings/
│   └── .gitignore
│
└── [Other folders: Library, Logs, UserSettings, etc.]
```

---

## 🔄 Recommended Reading Order

### For New Users
1. [README.md](README.md) (5 min)
2. [QUICKSTART.md](QUICKSTART.md) (10 min)
3. Run the system
4. [FAQ.md](FAQ.md) (as needed)

### For Installation
1. [INSTALLATION.md](INSTALLATION.md) - Choose your OS
2. Follow step-by-step
3. Test with Quick Start

### For Customization
1. [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md#-configuration)
2. Edit [config.py](RoFormer_Communication_Pipeline/config.py)
3. [FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md#-configuration)
4. Edit [JointReceiver.cs](FYP_Hand_Viz/Assets/Scripts/JointReceiver.cs)

### For Contributing
1. [CONTRIBUTING.md](CONTRIBUTING.md)
2. Fork and clone
3. Create feature branch
4. Make changes following guidelines
5. Submit pull request

---

## 📊 Documentation Overview

| Document | Length | Topics | For Whom |
|----------|--------|--------|----------|
| README.md | 300 lines | System overview, architecture, features | Everyone |
| QUICKSTART.md | 200 lines | Fast setup, testing, common issues | Users |
| INSTALLATION.md | 400 lines | Detailed per-OS setup, troubleshooting | Users installing |
| CONTRIBUTING.md | 250 lines | Dev workflow, standards, PR process | Contributors |
| FAQ.md | 350 lines | Q&A on setup, data, networking, etc. | Users needing answers |
| Python README.md | 450 lines | Complete pipeline API reference | Python devs |
| Unity README.md | 500 lines | Visualization, customization, builds | Unity devs |

**Total**: ~2500 lines of comprehensive documentation

---

## 🔍 Finding Information

### By Topic

#### **Installation & Setup**
- [INSTALLATION.md](INSTALLATION.md) - All OS-specific steps
- [QUICKSTART.md](QUICKSTART.md) - Fast setup
- [RoFormer_Communication_Pipeline/SETUP.md](RoFormer_Communication_Pipeline/SETUP.md) - Python-specific

#### **Configuration**
- [config.py](RoFormer_Communication_Pipeline/config.py) - Python settings
- [JointReceiver.cs](FYP_Hand_Viz/Assets/Scripts/JointReceiver.cs) - Unity settings
- [FAQ.md - Configuration](FAQ.md#data--configuration) - Common config questions

#### **Troubleshooting**
- [FAQ.md](FAQ.md) - Common issues and solutions
- [RoFormer README - Troubleshooting](RoFormer_Communication_Pipeline/README.md#-troubleshooting)
- [FYP_Hand_Viz README - Troubleshooting](FYP_Hand_Viz/README.md#-troubleshooting)

#### **Networking**
- [config.py - Network Configuration](RoFormer_Communication_Pipeline/config.py) - Network settings
- [README.md - Connection Details](README.md#-connection-details) - Protocol
- [FAQ.md - Networking](FAQ.md#networking--communication) - Network Q&A

#### **Development**
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution workflow
- [RoFormer_Communication_Pipeline/README.md](RoFormer_Communication_Pipeline/README.md) - Python API
- [FYP_Hand_Viz/README.md](FYP_Hand_Viz/README.md) - Unity scripting

#### **Performance**
- [FAQ.md - Performance](FAQ.md#performance--optimization)
- [RoFormer README - Benchmarks](RoFormer_Communication_Pipeline/README.md#-performance-benchmarks)
- [FYP_Hand_Viz README - Optimization](FYP_Hand_Viz/README.md#-performance-optimization)

---

## 💡 Tips for Getting the Most from This Documentation

1. **Use Ctrl+F (Cmd+F on Mac)** to search within documents
2. **Start with README.md** for high-level overview
3. **Go to QUICKSTART.md** to get running fast
4. **Check FAQ.md** before opening an issue
5. **Read project-specific README** for detailed information
6. **Follow INSTALLATION.md** exactly for your OS

---

## 🔗 External Resources

### Official Documentation
- [PyTorch Documentation](https://pytorch.org/docs/)
- [Unity Manual](https://docs.unity3d.com/Manual/)
- [Python Documentation](https://docs.python.org/3/)
- [GitHub Markdown Guide](https://guides.github.com/features/mastering-markdown/)

### Related Projects
- [Ninapro Dataset](https://ninapro.iit.unimi.it/)
- [Transformer Architecture Paper](https://arxiv.org/abs/1706.03762)
- [Rotary Position Embeddings Paper](https://arxiv.org/abs/2104.09864)

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | June 2024 | Initial comprehensive documentation release |

---

## 🆘 Can't Find What You Need?

1. **Check FAQ.md** - Most common questions answered
2. **Search GitHub Issues** - Others may have asked
3. **Read relevant project README** - Python or Unity specific docs
4. **Open a Discussion** - Ask the community
5. **Create an Issue** - Report a bug or request feature

---

## 📄 License

All documentation is provided under the same license as the project.
See [LICENSE](RoFormer_Communication_Pipeline/LICENSE) for details.

---

**Last Updated**: June 2024  
**Total Documentation**: ~2500 lines  
**Languages Covered**: Windows, macOS, Linux  
**For Questions**: Check FAQ.md or open GitHub Issue  
