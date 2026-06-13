# Contributing Guide

Thank you for your interest in contributing to the sEMG-to-Hand project! This guide will help you get started.

## Getting Started

### Fork and Clone
1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/sEMG-to-Hand.git
   cd sEMG-to-Hand
   ```
3. Add upstream remote:
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/sEMG-to-Hand.git
   ```

### Set Up Development Environment

#### Python Development
```bash
cd RoFormer_Communication_Pipeline
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
pip install -e .  # Install in development mode
```

#### Unity Development
1. Install Unity 2022 LTS
2. Open FYP_Hand_Viz project in Unity
3. Switch to a development branch

## Development Workflow

### 1. Create a Feature Branch
```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/bug-description
```

### 2. Make Your Changes

#### For Python Code
- Follow PEP 8 style guide
- Add docstrings to functions
- Write tests for new features
- Keep functions focused and small

**Example:**
```python
def preprocess_emg(signal, fs=2000, lowcut=5, highcut=500):
    """
    Preprocess raw EMG signal.
    
    Args:
        signal (np.ndarray): Raw EMG signal [samples, channels]
        fs (int): Sampling frequency in Hz
        lowcut (int): Bandpass filter lower cutoff
        highcut (int): Bandpass filter upper cutoff
    
    Returns:
        np.ndarray: Preprocessed signal [samples, channels]
    """
    # Implementation
    return processed_signal
```

#### For Unity Code
- Follow C# naming conventions (PascalCase for classes, camelCase for variables)
- Add comments for complex logic
- Use Inspector for configuration where possible
- Keep scripts focused on single responsibility

**Example:**
```csharp
/// <summary>
/// Receives joint angles via UDP and updates hand pose
/// </summary>
public class JointReceiver : MonoBehaviour
{
    [SerializeField]
    private int udpPort = 8051;
    
    /// <summary>Updates hand transforms based on angles</summary>
    private void UpdateHandPose(float[] angles) { }
}
```

### 3. Test Your Changes

#### Python Testing
```bash
cd RoFormer_Communication_Pipeline
python -m pytest tests/
python pipeline.py --test-mode
```

#### Unity Testing
1. Open Test Runner: Window → Testing → Test Runner
2. Write unit tests for critical functionality
3. Run Play mode tests

### 4. Commit Your Changes

Write clear, descriptive commit messages:
```bash
git add .
git commit -m "Add feature: UDP timeout configuration"
```

**Commit message format:**
- `feature: Add X functionality`
- `fix: Resolve issue with X`
- `docs: Update README for X`
- `refactor: Improve X performance`
- `test: Add tests for X`

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub with:
- Clear title describing the change
- Description of what changed and why
- Reference to related issues (#123)
- Screenshots/videos for UI changes

## Coding Standards

### Python
- **Style**: PEP 8 (use `black` for formatting)
- **Type hints**: Use type annotations where helpful
- **Documentation**: Docstrings for all public functions
- **Testing**: Aim for >80% coverage

### C# / Unity
- **Style**: C# conventions (PascalCase classes, camelCase members)
- **Null safety**: Always check for null before use
- **Performance**: Profile before optimizing; avoid frame-rate drops
- **Comments**: Explain WHY, not WHAT

## Project Guidelines

### Adding New Features

**For Signal Processing:**
1. Add to `RoFormer_Communication_Pipeline/`
2. Update `config.py` with any new parameters
3. Update `README.md` with documentation
4. Add usage example in `pipeline.py`

**For Visualization:**
1. Create script in `FYP_Hand_Viz/Assets/Scripts/`
2. Add UI elements in scene if needed
3. Document in `FYP_Hand_Viz/README.md`
4. Test in Play mode

### Documentation

- **README**: Keep updated with features and requirements
- **Code comments**: Explain complex algorithms
- **Docstrings**: Document all public APIs
- **Examples**: Provide code examples for complex features

### Performance

- **Python**: Profile using `cProfile` or `py-spy`
- **Unity**: Use Profiler window (Window → Analysis → Profiler)
- **Network**: Minimize UDP packet size and frequency
- **Real-time**: Maintain 50+ Hz for smooth visualization

## Reporting Issues

When reporting bugs:
1. Include OS, Python version, Unity version
2. Provide minimal reproducible example
3. Include error messages and stack traces
4. Describe expected vs actual behavior

**Issue template:**
```markdown
**Describe the bug:**
[Clear description]

**Steps to reproduce:**
1. ...
2. ...

**Expected behavior:**
[What should happen]

**Actual behavior:**
[What actually happens]

**Environment:**
- OS: Windows/macOS/Linux
- Python: 3.8/3.9/3.10/3.11
- Unity: 2022 LTS
```

## Code Review Process

1. **Automated checks**: All tests must pass
2. **Code review**: At least one maintainer reviews
3. **Feedback**: Respond to review comments
4. **Approval**: Approved by maintainers
5. **Merge**: Squash and merge to main branch

## Getting Help

- **Questions**: Open a Discussion on GitHub
- **Bugs**: Open an Issue with details
- **Ideas**: Start a Discussion first
- **Chat**: Check pinned messages in project wiki

## Merge Checklist

Before your PR is merged, ensure:
- [ ] All tests pass
- [ ] Code follows style guidelines
- [ ] Documentation is updated
- [ ] Commit messages are clear
- [ ] No breaking changes without discussion
- [ ] At least one approval from maintainers

## Code of Conduct

Be respectful and constructive:
- Welcome all skill levels
- Help newer contributors
- Discuss disagreements politely
- Focus on ideas, not people

## Resources

- [Git tutorial](https://git-scm.com/doc)
- [Python style guide (PEP 8)](https://pep8.org/)
- [C# conventions](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [Unity best practices](https://docs.unity3d.com/Manual/BestPracticeGuides.html)

## Questions?

Feel free to:
1. Check existing issues and discussions
2. Open a new Discussion
3. Comment on related PRs
4. Ask in the community

Thank you for contributing! 🙏

---

**Last Updated**: June 2024
