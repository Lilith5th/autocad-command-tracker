# AutoCAD Command Tracking System

A comprehensive AutoCAD plugin that tracks command usage, provides keyboard shortcuts, and displays real-time analytics.

## Features

### Core Functionality
- **Real-time command tracking** - Monitors AutoCAD commands as you use them
- **Keyboard shortcuts** - Ctrl+Q through Ctrl+C for quick command access (3x3 grid layout)
- **Command aliases** - Uses short forms like PL for POLYLINE, C for CIRCLE
- **Usage analytics** - Tracks frequency, patterns, and command sequences
- **Persistent storage** - Data saved between sessions

### User Interface
- **Dual UI options**: WPF and WinForms versions for maximum compatibility
- **Dark/Light themes** - Switch between themes in settings
- **Window transparency** - Adjustable opacity for non-intrusive use
- **Resizable windows** - Customize size and position
- **Red border indicator** - Shows when keyboard override is disabled

### Advanced Features
- **Ctrl+` toggle** - Enable/disable keyboard shortcuts on demand
- **Window synchronization** - Follows AutoCAD minimize/maximize behavior
- **Dimensional input parsing** - Captures coordinates, distances, and variables
- **Confirmed inputs only** - Records user-confirmed values, not preview data
- **Multi-profile support** - Different settings for different workflows
- **Import/Export** - Backup and share your command data

## Quick Start

### Installation
1. Compile the project (see [COMPILATION_GUIDE.md](COMPILATION_GUIDE.md))
2. Load the DLL in AutoCAD using `NETLOAD`
3. Run `CMDTRACK` (WPF) or `CMDTRACKWF` (WinForms)

### Basic Usage
```
CMDTRACK        - Start WPF version
CMDTRACKWF      - Start WinForms version  
CMDTRACK_STOP   - Stop tracking
Ctrl+`          - Toggle keyboard override on/off
Ctrl+Q to C     - Execute commands 1-9
```

### Keyboard Layout
Commands are mapped to a 3x3 grid for international keyboard compatibility:
```
Q  W  E    (Commands 1-3)
A  S  D    (Commands 4-6)  
Z  X  C    (Commands 7-9)
```

## Architecture

### Project Structure
```
2017_test_binding/
├── Models/                 - Data models (CommandData, InputValue)
├── Services/              - Business logic (Analytics, Persistence, Aliases)
├── ViewModels/            - MVVM view models for WPF
├── Views/                 - UI components (WPF and WinForms)
├── Converters/            - WPF data binding converters
├── MyCommandsEnhanced.cs  - Main WPF implementation
├── MyCommandsWinForms.cs  - Main WinForms implementation
└── *.cs                   - Supporting classes and utilities
```

### Key Components
- **CommandData** - Stores command usage statistics and input values
- **DataPersistenceService** - JSON-based data storage in %AppData%
- **CommandAnalyticsService** - Usage pattern analysis and statistics
- **CommandAliasService** - Command name to alias mapping

## Commands Reference

| Command | Description |
|---------|-------------|
| `CMDTRACK` | Start WPF version with modern UI |
| `CMDTRACKWF` | Start WinForms version (better AutoCAD compatibility) |
| `CMDTRACK_STOP` | Stop WPF tracking |
| `CMDTRACKWF_STOP` | Stop WinForms tracking |
| `CMDTRACK_WINDOWSYNC` | Toggle window synchronization |

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+`` | Toggle keyboard override on/off |
| `Ctrl+Q` | Execute command #1 (most used) |
| `Ctrl+W` | Execute command #2 |
| `Ctrl+E` | Execute command #3 |
| `Ctrl+A` | Execute command #4 |
| `Ctrl+S` | Execute command #5 |
| `Ctrl+D` | Execute command #6 |
| `Ctrl+Z` | Execute command #7 |
| `Ctrl+X` | Execute command #8 |
| `Ctrl+C` | Execute command #9 |

*Commands are sorted by usage frequency, so your most-used commands get the easiest shortcuts.*

## Visual Indicators

- **Normal border** - Keyboard override is active
- **Red border** - Keyboard override is disabled (Ctrl+ keys pass to AutoCAD)
- **Window title** - Shows "(Override OFF)" when shortcuts are disabled

## Data Storage

Data is automatically saved to:
```
%AppData%\AutoCADCommandTracker\
├── command_data.json       - Command usage statistics
├── settings.json          - User preferences  
└── profiles/              - Profile-specific data
```

## Compatibility

- **AutoCAD Versions**: 2015+ (.NET Framework 4.5+)
- **Operating Systems**: Windows 7/8/10/11
- **UI Frameworks**: WPF and WinForms options available
- **Keyboard Layouts**: International layout support (QWERTY, QWERTZ, etc.)

## Development

### Building
See [COMPILATION_GUIDE.md](COMPILATION_GUIDE.md) for detailed build instructions.

### Architecture Notes
- WPF version uses MVVM pattern with data binding
- WinForms version uses traditional event-driven architecture  
- Both versions share common models and services
- Event-driven design with AutoCAD API integration
- Defensive programming with comprehensive error handling

## License

This project is provided as-is for educational and development purposes.

## Contributing

The codebase has been analyzed and contains approximately 40-45% redundancy between WPF and WinForms implementations. Future improvements could include:
- Extracting common base classes
- Consolidating duplicate event handling logic
- Simplifying the analytics system
- Implementing proper dependency injection

