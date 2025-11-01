using System.Windows;
using System.Windows.Input;

namespace AutoCadCommandTracker.Views
{
    public partial class ShortcutInputWindow : Window
    {
        public string ShortcutKey { get; private set; }

        public ShortcutInputWindow(string commandName)
        {
            InitializeComponent();
            CommandNameRun.Text = commandName;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ignore modifier keys alone
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.System)
            {
                return;
            }

            // Build shortcut string
            string shortcut = "";

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                shortcut += "Ctrl+";
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                shortcut += "Alt+";
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                shortcut += "Shift+";

            // Add the main key
            shortcut += e.Key.ToString();

            // Display the shortcut
            ShortcutTextBlock.Text = shortcut;
            ShortcutKey = shortcut;
            OkButton.IsEnabled = true;

            e.Handled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ShortcutKey) || ShortcutKey == "Press keys...")
            {
                MessageBox.Show("Please press a key combination first.", "No Shortcut",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
