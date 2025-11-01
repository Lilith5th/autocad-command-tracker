using System;
using System.Windows.Forms;

namespace AutoCadCommandTracker.Views
{
    public partial class ShortcutInputDialog : Form
    {
        public string ShortcutKey { get; private set; }
        private string _commandName;

        public ShortcutInputDialog(string commandName)
        {
            _commandName = commandName;
            InitializeComponent();
            SetupDialog();
        }

        private void SetupDialog()
        {
            this.Text = "Set Custom Shortcut";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new System.Drawing.Size(400, 200);
            this.KeyPreview = true;

            // Instruction label
            var instructionLabel = new Label
            {
                Text = $"Press the key combination you want to use for '{_commandName}'",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(350, 40),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };

            // Shortcut display
            _shortcutTextBox.Location = new System.Drawing.Point(20, 70);
            _shortcutTextBox.Size = new System.Drawing.Size(350, 30);
            _shortcutTextBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            _shortcutTextBox.ReadOnly = true;
            _shortcutTextBox.TextAlign = HorizontalAlignment.Center;
            _shortcutTextBox.Text = "Press keys...";

            // OK button
            _okButton.Text = "OK";
            _okButton.Location = new System.Drawing.Point(200, 120);
            _okButton.Size = new System.Drawing.Size(75, 25);
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Enabled = false;
            _okButton.Click += OkButton_Click;

            // Cancel button
            _cancelButton.Text = "Cancel";
            _cancelButton.Location = new System.Drawing.Point(290, 120);
            _cancelButton.Size = new System.Drawing.Size(75, 25);
            _cancelButton.DialogResult = DialogResult.Cancel;

            this.Controls.Add(instructionLabel);
            this.Controls.Add(_shortcutTextBox);
            this.Controls.Add(_okButton);
            this.Controls.Add(_cancelButton);
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            // Subscribe to key events
            this.KeyDown += ShortcutInputDialog_KeyDown;
        }

        private void ShortcutInputDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // Ignore modifier keys alone
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey ||
                e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
            {
                return;
            }

            // Build shortcut string
            string shortcut = "";

            if (e.Control)
                shortcut += "Ctrl+";
            if (e.Alt)
                shortcut += "Alt+";
            if (e.Shift)
                shortcut += "Shift+";

            // Add the main key
            shortcut += e.KeyCode.ToString();

            // Display the shortcut
            _shortcutTextBox.Text = shortcut;
            ShortcutKey = shortcut;
            _okButton.Enabled = true;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ShortcutKey) || ShortcutKey == "Press keys...")
            {
                MessageBox.Show("Please press a key combination first.", "No Shortcut",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
