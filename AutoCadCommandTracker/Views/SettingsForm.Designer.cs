namespace AutoCadCommandTracker.Views
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._tabControl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();

            //
            // _tabControl
            //
            this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl.Location = new System.Drawing.Point(0, 0);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(400, 260);
            this._tabControl.TabIndex = 0;

            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";

            // Note: Tab creation and population happens in SettingsForm.cs constructor
            // after InitializeComponent() is called

            this.ResumeLayout(false);
        }

        // Note: CreateAppearanceTab, CreateBehaviorTab, and CreateShortcutsTab 
        // are implemented in the main SettingsForm.cs file

        #endregion

        // Main control - initialized in designer
        private System.Windows.Forms.TabControl _tabControl;

        // Other controls - initialized programmatically in CreateTabs()
        private System.Windows.Forms.RadioButton _lightThemeRadio;
        private System.Windows.Forms.RadioButton _darkThemeRadio;
        private System.Windows.Forms.TrackBar _opacityTrackBar;
        private System.Windows.Forms.Label _opacityLabel;
        private System.Windows.Forms.CheckBox _resizableCheckBox;
        private System.Windows.Forms.NumericUpDown _maxValuesNumeric;
        private System.Windows.Forms.CheckBox _trackTimeCheckBox;
        private System.Windows.Forms.CheckBox _trackSequencesCheckBox;
        private System.Windows.Forms.TextBox[] _shortcutTextBoxes;
        private System.Windows.Forms.Button _resetShortcutsButton;
    }
}