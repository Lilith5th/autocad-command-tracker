# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AutoCAD Command Tracker is an AutoCAD plugin (.NET Framework 4.8) that tracks command usage, provides keyboard shortcuts (Ctrl+Q through Ctrl+C), and displays real-time analytics. It offers both WPF and WinForms UI implementations for maximum compatibility.

## Build Commands

### Visual Studio
```bash
# Open solution
AutoCadCommandTracker.sln

# Build both projects
msbuild AutoCadCommandTracker.sln /p:Configuration=Debug
msbuild AutoCadCommandTracker.sln /p:Configuration=Release
```

### Output Files
- WPF version: `AutoCadCommandTracker\bin\Debug\AutoCadCommandTracker.dll`
- WinForms version: `AutoCadCommandTracker\bin\Debug\AutoCadCommandTracker.WinForms.dll`

### Testing in AutoCAD
```
NETLOAD               # Load the DLL
CMDTRACK              # Start WPF version
CMDTRACKWF            # Start WinForms version
CMDTRACK_STOP         # Stop WPF tracking
CMDTRACKWF_STOP       # Stop WinForms tracking
```

## Architecture

### Code Organization Strategy

The project uses **inheritance-based code sharing** to eliminate redundancy between WPF and WinForms implementations:

- **CommandTrackerBase** (base class): Contains ~800 lines of shared logic including command tracking, keyboard shortcut management, AutoCAD event handling, and data persistence
- **WpfCommandTracker** (derived): WPF-specific UI, window state synchronization, and MVVM patterns
- **WinFormsCommandTracker** (derived): WinForms-specific UI and event-driven architecture

### Key Architectural Patterns

**Dual UI Framework Support**
- Both WPF and WinForms implementations share the same Models and Services
- WPF uses MVVM pattern with data binding (ViewModels/)
- WinForms uses traditional event-driven architecture
- Common base class (CommandTrackerBase) eliminates code duplication

**AutoCAD Integration**
- Implements `IExtensionApplication` for AutoCAD lifecycle management
- Command methods decorated with `[CommandMethod]` attribute
- Event-driven tracking via AutoCAD's `CommandWillStart` and `CommandEnded` events
- Keyboard override system using Windows message interception (PreTranslateMessage)

**Data Persistence**
- JSON-based storage using Newtonsoft.Json
- Data stored in `%AppData%\AutoCADCommandTracker\`
- DataPersistenceService handles all file I/O
- Support for multiple profiles (profile-specific data in `profiles/` subdirectory)

### Critical Components

**CommandTrackerBase.cs** (`AutoCadCommandTracker/CommandTrackerBase.cs`)
- Abstract base class with shared tracking logic
- Keyboard shortcut system (Ctrl+Q through Ctrl+C mapped to 3x3 grid: Q/W/E, A/S/D, Z/X/C)
- AutoCAD event registration and handling
- Thread-safe command data management with lock objects

**Models/**
- `CommandData`: Stores command usage stats, input values, last used timestamp, and command sequences
- `InputValue`: Tracks user inputs with usage counts
- `DisplayItem`: UI representation of command data

**Services/**
- `DataPersistenceService`: JSON serialization/deserialization to `%AppData%`
- `CommandAliasService`: Maps full command names to short forms (e.g., POLYLINE → PL)

**ViewModels/** (WPF only)
- `MainViewModel`: Base MVVM implementation
- `MainViewModelEnhanced`: Extended with additional tracking features

**Views/**
- WPF: MainWindowEnhanced.xaml, SettingsWindow.xaml, StatisticsWindow.xaml
- WinForms: MainFormEnhanced.cs, SettingsForm.cs, StatisticsForm.cs

### AutoCAD Reference Configuration

The project references AutoCAD 2023 assemblies:
```
C:\Program Files\Autodesk\AutoCAD 2023\
  - accoremgd.dll
  - acdbmgd.dll
  - acmgd.dll
  - AdWindows.dll
```

Update .csproj files if using a different AutoCAD version.

## Development Guidelines

### Keyboard Shortcut System
- Uses 3x3 QWERTY grid for international compatibility: Q/W/E (top), A/S/D (middle), Z/X/C (bottom)
- Ctrl+` toggles keyboard override on/off
- When disabled, red border appears and shortcuts pass through to AutoCAD
- Shortcuts execute the top 9 most-used commands

### Thread Safety
- All command data modifications must use `_lockObject` for synchronization
- UI updates from AutoCAD events must use proper dispatcher/invoke patterns

### AutoCAD Event Handling
- Register events in `Initialize()`, unregister in `Terminate()`
- Use defensive programming with try-catch blocks
- Always check for null documents/editors before accessing AutoCAD API

### Data Model Consistency
- CommandData tracks: name, usage count, input values, last used time, followed commands, category
- Always call `IncrementUsage()` to update both count and timestamp
- InputValues track both the value and usage frequency

### UI Framework Considerations
- WPF version uses data binding - update ViewModel properties to reflect changes
- WinForms version requires manual UI updates via `Invoke()`
- Both versions share the same underlying data models

## Common Pitfalls

1. **AutoCAD Reference Paths**: If compilation fails with missing references, update the HintPath in .csproj files to match your AutoCAD installation directory

2. **XAML Build Issues**: For WPF version, always rebuild entire solution if XAML files change - they need MSBuild preprocessing

3. **Keyboard Shortcuts Not Working**: Ensure PreTranslateMessage is properly registered and that keyboard override is enabled (no red border)

4. **Data Not Persisting**: Check %AppData%\AutoCADCommandTracker\ directory exists and has write permissions

5. **Window State Issues**: WPF version has window synchronization with AutoCAD - ensure timer cleanup in Terminate()

## Dependencies

- .NET Framework 4.8
- Newtonsoft.Json 13.0.3 (NuGet package)
- AutoCAD ObjectARX 2023 SDK (or compatible version)
- Windows Forms (System.Windows.Forms)
- WPF (PresentationCore, PresentationFramework, WindowsBase)

## File Structure Notes

The git status shows deleted files from old project structure (2017_test_binding/). The current active project is in AutoCadCommandTracker/ directory with reorganized structure and improved architecture using base class inheritance pattern.
