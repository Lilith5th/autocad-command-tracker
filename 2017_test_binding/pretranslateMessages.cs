using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Diagnostics;

using System.Globalization;
using System.Text.RegularExpressions;


namespace _2017_test_binding
{
    public partial class MyCommands : IMessageFilter
    {
        /// <summary>
        /// pretranslating autocad messages 
        /// </summary>
        #region preTranslateAUTOCAD messages 
        #region keystroke codes
        // Constants for our keystroke interpretation code

        private const int WM_KEYDOWN = 256;
        private const int WM_KEYUP = 257;
        private const int WM_CHAR = 258;
        private const int KEY_PRESSED = 128;



        // 37 - left arrow (no char, keydown/up)
        // 38 - up arrow (no char, keydown/up)
        // 39 - right arrow (no char, keydown/up)
        // 40 - down arrow (no char, keydown/up)
        // 46 - delete (no char, keydown/up)

        private static readonly List<int> cancelKeys = new List<int> { 37, 38, 39, 40, 46 };

        // 13 - enter (char + keydown/up)
        // 32 - space (char + keydown/up)

        private static readonly List<int> enterKeys = new List<int> { 13, 32 };

        //ovo su keycodovi za brojeve i numkeyse
        private static readonly List<int> number = new List<int> { 48, 49, 50, 51, 52, 53, 54, 55, 56, 57,/*numkeys*/ 96, 97, 98, 99, 100, 101, 102, 103, 104, 105 };
        #endregion

        private List<string> numKeyStrokes = new List<string>();

        private void OnPreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
        {
            KeysConverter kc = new KeysConverter();

            if (Prompt_is_on_main != true)
            {

                var wp = unchecked((int)e.Message.wParam.ToInt64());
                // if number send our characters to a list
                if (e.Message.message == WM_KEYDOWN && number.Contains(wp))
                {
                    numKeyStrokes.Add(kc.ConvertToString(wp));
                }

                // if enter keys add number to list
                if (e.Message.message == WM_KEYDOWN && enterKeys.Contains(wp))
                {
                    //get our characters and convert them to a number
                    string number = string.Join("", numKeyStrokes.ToArray());

                    var style = NumberStyles.Float | NumberStyles.AllowThousands;
                    var culture = CultureInfo.InvariantCulture;
                    double num;
                    if (double.TryParse(number, style, culture, out num))
                    {
                        last_entered_number = num;
                        NumberEntered_EventHandler(last_entered_number);
                        numKeyStrokes.Clear();
                    }
                }

                //if backspace
                if (e.Message.message == WM_KEYDOWN && wp == 8)
                // If we have a backspace character, remove the last
                // entry in our character list, otherwise add the
                // character to the list
                {
                    if (numKeyStrokes.Count > 0)
                        numKeyStrokes.RemoveAt(numKeyStrokes.Count - 1);
                }
            }
        }


        private void NumberEntered_EventHandler(double number)
        {
            //moramo pazit da ne dodamo dva puta isti broj, tj. cistiti buffer--to se da u propertyu dodat
            commands_data curComData = main_commands_values.Find(i => i.command == active_main_command);
            int curComIndex = main_commands_values.FindIndex(i => i.command == active_main_command);

            int curNumberPos = -1;
            curNumberPos = curComData.added_numbers.FindIndex(i => i.number == number);

            if (curNumberPos < 0)
            {
                dimensionData numToAdd = new dimensionData(number);
                main_commands_values[curComIndex].added_numbers.Add(numToAdd);
            }
            else
            {
                dimensionData curNumberData = curComData.added_numbers[curNumberPos];
                curNumberData.increase_counter();
                main_commands_values[curComIndex].added_numbers[curNumberPos] = curNumberData;
            }
        }
        #endregion

        //ovo jos nije implementano

        private bool keyboardFilterEnabled = false;
        public bool KeyboardFilterEnabled
        {
            get { return keyboardFilterEnabled; }
            set
            {
                keyboardFilterEnabled = value;
                MyCommands filter = this;

                if (keyboardFilterEnabled == true)
                    System.Windows.Forms.Application.AddMessageFilter(filter);
                else
                    System.Windows.Forms.Application.RemoveMessageFilter(filter);
            }
        }


        public enum VirtualKeys : int
        {
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04,
            VK_XBUTTON1 = 0x05,
            VK_XBUTTON2 = 0x6,
            VK_BACK = 0x08,
            VK_TAB = 0x09,
            VK_RETURN = 0x0D,
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_ESCAPE = 0x1B,
            VK_SPACE = 0x20,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_DELETE = 0x2E,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
        }

        List<char> myShortcutKeys = new List<char>()
            { 'Q', 'W','E', 'A', 'S', 'D', 'Y', 'X', 'C' };

        //public const int WM_KEYDOWN = 0x0100;
        public bool PreFilterMessage(ref Message m)
        {
            if ((m.Msg == WM_KEYDOWN)) /*0x0100 256 */
            {
                if ((Control.ModifierKeys & System.Windows.Forms.Keys.Control) > 0)
                {
                    Keys kc = (Keys)(int)m.WParam & Keys.KeyCode;

                    if (myShortcutKeys.Contains((char)kc))
                    {
                        int shortcutIndex = myShortcutKeys.FindIndex(o => o == (char)kc);
                        sendCommand(shortcutIndex+1);
                        return true;
                    }

                }
            }

            //// ovo je relikt koji ne radi
            //if (m.WParam.ToInt32() == Convert.ToInt32(VirtualKeys.VK_CONTROL) && myShortcutKeys.Contains((char)kc))
            //{
            //    if (myShortcutKeys.Contains((char)kc))
            //    {
            //        int shortcutIndex = myShortcutKeys.FindIndex(o => o == (char)kc);
            //        sendCommand(shortcutIndex);
            //        return true;
            //    }
            //}

            return false;
        }


        public void sendCommand(int shortcutIndex)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            string command = window_display_data[shortcutIndex - 1];
            doc.SendStringToExecute(command + " ", true, false, false);
        }
    }
}
