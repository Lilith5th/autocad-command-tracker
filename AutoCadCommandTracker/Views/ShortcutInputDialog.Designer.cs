namespace AutoCadCommandTracker.Views
{
    partial class ShortcutInputDialog
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
            this._shortcutTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // Note: Control layout is done in SetupDialog() method

            //
            // ShortcutInputDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Name = "ShortcutInputDialog";
            this.Text = "Set Custom Shortcut";

            this.ResumeLayout(false);
        }

        #endregion

        // Controls initialized in SetupDialog()
        private System.Windows.Forms.TextBox _shortcutTextBox = new System.Windows.Forms.TextBox();
        private System.Windows.Forms.Button _okButton = new System.Windows.Forms.Button();
        private System.Windows.Forms.Button _cancelButton = new System.Windows.Forms.Button();
    }
}
