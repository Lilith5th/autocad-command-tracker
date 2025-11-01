using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;
using AutoCadCommandTracker.Views;

namespace AutoCadCommandTracker
{
    /// <summary>
    /// WinForms implementation of AutoCAD Command Tracker
    /// Inherits shared functionality from CommandTrackerBase to eliminate code duplication
    /// </summary>
    public class WinFormsCommandTracker : CommandTrackerBase
    {
        private MainFormEnhanced _window;
        
        // Static reference for settings updates
        public static WinFormsCommandTracker CurrentInstance { get; private set; }

        #region IExtensionApplication Override

        public override void Terminate()
        {
            try
            {
                if (_window != null)
                {
                    _window.Close();
                    _window = null;
                }
                
                // Clear current instance
                if (CurrentInstance == this)
                    CurrentInstance = null;
                
                base.Terminate();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during WinForms termination: {ex.Message}");
            }
        }

        #endregion

        [CommandMethod("CMDTRACKWF", CommandFlags.Session)]
        public void StartCommandTrackingWinForms()
        {
            try
            {
                // Set current instance for settings updates
                CurrentInstance = this;
                
                // Ensure services are initialized
                if (_persistenceService == null)
                {
                    Initialize();
                }

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var ed = doc.Editor;

                // Create and show window
                if (_window == null)
                {
                    _window = new MainFormEnhanced();
                    _window.Show();
                    _window.StartTracking();
                    UpdateWindowColorForOverrideState(); // Update window state based on override
                    Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
                }

                // Register event handlers (includes keyboard shortcuts)
                RegisterEventHandlers(doc);

                // Initialize default commands if needed
                InitializeDefaultCommands();

                // Update display with current data
                OnDisplayUpdateNeeded();

                ed.WriteMessage($"\nWinForms command tracking started. {_commandDataList.Count} commands available.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting command tracking: {ex.Message}");
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nError starting command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACKWF_STOP", CommandFlags.Session)]
        public void StopCommandTrackingWinForms()
        {
            try
            {
                if (_window != null)
                {
                    _window.StopTracking();
                    _window.Close();
                    _window = null;
                }

                CleanupKeyboardShortcuts();
                UnregisterEventHandlers();
                SaveCommandData();

                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage("\nWinForms command tracking stopped.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACKWF_ADD", CommandFlags.Session)]
        public void AddDefaultCommands()
        {
            try
            {
                if (_persistenceService == null)
                {
                    Initialize();
                }

                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                
                // Add all default commands
                foreach (var category in _startupCommands)
                {
                    foreach (var cmd in category.Value)
                    {
                        var cmdData = GetOrCreateCommand(cmd);
                        cmdData.IncrementUsage();
                        AddCommonInputValues(cmdData, cmd);
                    }
                }

                ed?.WriteMessage($"\nAdded {_commandDataList.Count} commands with input values.");
                OnDisplayUpdateNeeded();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding default commands: {ex.Message}");
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nError adding commands: {ex.Message}");
            }
        }

        #region WinForms-Specific Event Handlers
        
        protected override void RegisterEventHandlers(Document doc)
        {
            try
            {
                base.RegisterEventHandlers(doc);
                
                // Add WinForms-specific editor events
                var ed = doc.Editor;
                ed.PromptingForPoint += OnPromptingForPoint;
                ed.PromptingForDistance += OnPromptingForDistance;
                ed.PromptingForInteger += OnPromptingForInteger;
                ed.PromptingForDouble += OnPromptingForDouble;
                ed.PromptingForString += OnPromptingForString;
                ed.PromptingForKeyword += OnPromptingForKeyword;
                ed.PromptingForAngle += OnPromptingForAngle;
                ed.PromptingForCorner += OnPromptingForCorner;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering WinForms event handlers: {ex.Message}");
            }
        }

        protected override void UnregisterEventHandlers()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    // Remove WinForms-specific editor events
                    var ed = doc.Editor;
                    ed.PromptingForPoint -= OnPromptingForPoint;
                    ed.PromptingForDistance -= OnPromptingForDistance;
                    ed.PromptingForInteger -= OnPromptingForInteger;
                    ed.PromptingForDouble -= OnPromptingForDouble;
                    ed.PromptingForString -= OnPromptingForString;
                    ed.PromptingForKeyword -= OnPromptingForKeyword;
                    ed.PromptingForAngle -= OnPromptingForAngle;
                    ed.PromptingForCorner -= OnPromptingForCorner;
                }
                
                base.UnregisterEventHandlers();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering WinForms event handlers: {ex.Message}");
            }
        }

        #endregion

        #region Base Class Overrides
        
        protected override void OnCommandWillStart(object sender, CommandEventArgs e)
        {
            try
            {
                base.OnCommandWillStart(sender, e);
                
                // Add WinForms-specific behavior
                var command = GetOrCreateCommand(_activeCommand);
                AddCommonInputValues(command, _activeCommand);
                
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nCommand '{_activeCommand}' started - keyboard shortcuts disabled");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in WinForms OnCommandWillStart: {ex.Message}");
            }
        }
        
        protected override void OnCommandEnded(object sender, CommandEventArgs e)
        {
            base.OnCommandEnded(sender, e);
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' ended - keyboard shortcuts re-enabled");
        }
        
        protected override void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            base.OnCommandCancelled(sender, e);
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' cancelled - keyboard shortcuts re-enabled");
        }
        
        protected override void OnCommandFailed(object sender, CommandEventArgs e)
        {
            base.OnCommandFailed(sender, e);
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' failed - keyboard shortcuts re-enabled");
        }
        
        protected override void OnDisplayUpdateNeeded()
        {
            try
            {
                UpdateDisplay();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating WinForms display: {ex.Message}");
            }
        }
        
        protected override void OnOverrideStateChanged()
        {
            try
            {
                UpdateWindowColorForOverrideState();
                
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nKeyboard override {(_keyboardOverrideEnabled ? "enabled" : "disabled")}");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating WinForms override state: {ex.Message}");
            }
        }

        #endregion

        #region Prompt Events

        private void OnPromptingForPoint(object sender, PromptPointOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForDistance(object sender, PromptDistanceOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForAngle(object sender, PromptAngleOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForString(object sender, PromptStringOptionsEventArgs e)
        {
            if (e.Options.Keywords.Count > 0)
            {
                DisplayKeywords(e.Options.Keywords);
            }
            else
            {
                DisplayCommandValues();
            }
        }

        private void OnPromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            DisplayKeywords(e.Options.Keywords);
        }

        private void OnPromptingForCorner(object sender, PromptPointOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        #endregion

        #region WinForms-Specific Helper Methods

        private void UpdateDisplay()
        {
            try
            {
                if (_window != null && _commandDataList != null)
                {
                    var currentDataList = new List<CommandData>(_commandDataList);
                    
                    if (_isAtMainPrompt)
                    {
                        _window.UpdateDisplayData(currentDataList, true);
                    }
                    else
                    {
                        _window.UpdateDisplayData(currentDataList, false, _activeCommand);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating WinForms display: {ex.Message}");
            }
        }

        private void DisplayCommandValues()
        {
            if (_window != null && !string.IsNullOrEmpty(_activeCommand) && _commandDataList != null)
            {
                var command = GetOrCreateCommand(_activeCommand);
                var currentDataList = new List<CommandData>(_commandDataList);
                _window.UpdateDisplayData(currentDataList, false, _activeCommand);
            }
        }

        private void DisplayKeywords(KeywordCollection keywords)
        {
            // Convert keywords to display format
            // This could be enhanced to show keyword descriptions
        }

        private void AddCommonInputValues(CommandData command, string commandName)
        {
            // Add common input values based on command type
            switch (commandName.ToUpper())
            {
                case "OFFSET":
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("15", InputType.Number);
                    command.AddInputValue("20", InputType.Number);
                    command.AddInputValue("25", InputType.Number);
                    break;
                
                case "FILLET":
                case "CHAMFER":
                    command.AddInputValue("0", InputType.Number);
                    command.AddInputValue("2", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    break;
                
                case "SCALE":
                    command.AddInputValue("0.5", InputType.Number);
                    command.AddInputValue("1", InputType.Number);
                    command.AddInputValue("2", InputType.Number);
                    command.AddInputValue("1.5", InputType.Number);
                    break;
                
                case "ROTATE":
                    command.AddInputValue("90", InputType.Angle);
                    command.AddInputValue("180", InputType.Angle);
                    command.AddInputValue("270", InputType.Angle);
                    command.AddInputValue("45", InputType.Angle);
                    break;
                
                case "ARRAY":
                    command.AddInputValue("3", InputType.Number);
                    command.AddInputValue("4", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("6", InputType.Number);
                    break;
                
                case "CIRCLE":
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("25", InputType.Number);
                    command.AddInputValue("50", InputType.Number);
                    command.AddInputValue("100", InputType.Number);
                    break;
                
                case "TEXT":
                case "MTEXT":
                    command.AddInputValue("3.5", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("7", InputType.Number);
                    break;
                
                case "LAYER":
                    command.AddInputValue("0", InputType.Text);
                    command.AddInputValue("Dimensions", InputType.Text);
                    command.AddInputValue("Text", InputType.Text);
                    command.AddInputValue("Hatching", InputType.Text);
                    break;
                
                case "ZOOM":
                    command.AddInputValue("All", InputType.Text);
                    command.AddInputValue("Extents", InputType.Text);
                    command.AddInputValue("Window", InputType.Text);
                    command.AddInputValue("0.5x", InputType.Text);
                    command.AddInputValue("2x", InputType.Text);
                    break;
            }
        }


        #endregion

        #region WinForms-Specific Visual Feedback

        private void UpdateWindowColorForOverrideState()
        {
            if (_window == null) return;
            
            // Clean up any previous paint handlers
            _window.Paint -= OnWindowPaintRedBorder;
            
            if (_keyboardOverrideEnabled)
            {
                // Normal state - no special painting needed
                _window.Invalidate(); // Force repaint to remove any existing border
            }
            else
            {
                // Disabled state - add paint handler for red border
                _window.Paint += OnWindowPaintRedBorder;
                _window.Invalidate(); // Force repaint to show border
            }
        }
        
        private void OnWindowPaintRedBorder(object sender, PaintEventArgs e)
        {
            // Draw thick red border around the entire form
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 4))
            {
                var rect = e.ClipRectangle;
                e.Graphics.DrawRectangle(pen, 2, 2, _window.ClientSize.Width - 4, _window.ClientSize.Height - 4);
            }
        }

        #endregion
    }
}