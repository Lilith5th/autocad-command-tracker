namespace AutoCadCommandTracker.Views
{
    partial class StatisticsForm
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
            this._tabControl.Margin = new System.Windows.Forms.Padding(10);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(600, 500);
            this._tabControl.TabIndex = 0;

            // Note: Tab creation and population happens in StatisticsForm.cs constructor
            // after InitializeComponent() is called

            //
            // StatisticsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "StatisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Command Statistics";

            this.ResumeLayout(false);
        }

        #endregion

        // Main control - initialized in designer
        private System.Windows.Forms.TabControl _tabControl;

        // Other controls - initialized programmatically in CreateTabs()
        private System.Windows.Forms.Label _totalCommandsLabel;
        private System.Windows.Forms.Label _uniqueCommandsLabel;
        private System.Windows.Forms.Label _mostUsedLabel;
        private System.Windows.Forms.Label _avgPerHourLabel;
        private System.Windows.Forms.DataGridView _commandsGrid;
        private System.Windows.Forms.DataGridView _sequencesGrid;
    }
}