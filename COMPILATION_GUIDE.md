# Compilation Guide for Enhanced AutoCAD Command Tracker

## Prerequisites

1. **Visual Studio 2017 or later** (or Visual Studio Build Tools 2017+)
2. **.NET Framework 4.8** (already configured in project)
3. **AutoCAD ObjectARX 2017** SDK (paths already configured)
4. **NuGet Package Manager**

## Before Compiling

### 1. Install Required NuGet Package
The project requires Newtonsoft.Json for data persistence. You have two options:

#### Option A: Using Visual Studio
1. Open the solution in Visual Studio
2. Right-click on the solution → "Manage NuGet Packages for Solution"
3. Search for "Newtonsoft.Json" and install version 13.0.3

#### Option B: Using Package Manager Console
```powershell
Install-Package Newtonsoft.Json -Version 13.0.3
```

### 2. Verify AutoCAD References
Ensure the AutoCAD ObjectARX paths in the project file point to your installation:
- AcCoreMgd.dll
- AcDbMgd.dll  
- AcMgd.dll

Update paths in the .csproj file if needed.

## Compilation Steps

### Using Visual Studio
1. Open `2017_test_binding.sln`
2. Build → Rebuild Solution
3. Check for any compilation errors in the Error List

### Using MSBuild (Command Line)
```cmd
msbuild "2017_test_binding.sln" /p:Configuration=Debug
```

### Using .NET CLI (if converted to SDK-style project)
```cmd
dotnet build
```

## Expected Output

After successful compilation:
- **Debug build**: `bin\Debug\2017_test_binding.dll`
- **Release build**: `bin\Release\2017_test_binding.dll`

## Common Compilation Issues & Solutions

### 1. Missing References
**Error**: "The type or namespace name 'Newtonsoft' could not be found"
**Solution**: Install Newtonsoft.Json NuGet package

### 2. AutoCAD Reference Issues
**Error**: "Could not load file or assembly 'AcMgd'"
**Solution**: Update AutoCAD reference paths to match your installation

### 3. XAML Build Errors
**Error**: "The name 'X' does not exist in the namespace"
**Solution**: Rebuild the entire solution (XAML files need to be processed)

### 4. Missing nameof operator
**Error**: "The name 'nameof' does not exist in the current context"
**Solution**: Ensure C# language version is set to 6.0 or higher

## Project Structure Verification

Ensure all these files are included in the project:

### Core Files
- ✅ MyCommandsEnhanced.cs
- ✅ CompilationTest.cs

### Models
- ✅ Models/CommandData.cs
- ✅ Models/InputValue.cs

### ViewModels  
- ✅ ViewModels/MainViewModel.cs
- ✅ ViewModels/MainViewModelEnhanced.cs

### Views
- ✅ Views/MainWindowEnhanced.xaml
- ✅ Views/MainWindowEnhanced.xaml.cs
- ✅ Views/SettingsWindow.xaml
- ✅ Views/SettingsWindow.xaml.cs
- ✅ Views/StatisticsWindow.xaml
- ✅ Views/StatisticsWindow.xaml.cs

### Services
- ✅ Services/DataPersistenceService.cs
- ✅ Services/CommandAnalyticsService.cs

### Converters
- ✅ Converters/BoolToResizeModeConverter.cs

## Testing the Build

1. Load the compiled DLL in AutoCAD using NETLOAD command
2. Run the `CMDTRACK` command to start tracking
3. Verify the enhanced window appears with all new features

## Notes

- The enhanced version is fully backward compatible
- Original files (myClasses.cs, MainWindow.xaml) are preserved
- New features are in separate files and can be used independently
- Data persistence will create files in `%APPDATA%/AutoCADCommandTracker/`

## Troubleshooting

If compilation fails:
1. Clean and rebuild the entire solution
2. Verify all file paths in the project file
3. Check that all XAML files have their code-behind files properly linked
4. Ensure the Newtonsoft.Json package is properly restored

For runtime issues:
1. Check AutoCAD version compatibility
2. Verify all AutoCAD references are the correct version
3. Test with the original `pokretanje` command first to ensure basic functionality