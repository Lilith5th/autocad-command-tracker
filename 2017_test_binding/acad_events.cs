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



    public partial class MyCommands
    {
        List<string> startupCommands = new List<string>()
        {
            // Drawing Commands
            "PLINE", "LINE", "CIRCLE", "ARC", "ELLIPSE", "RECTANGLE",
            // Modify Commands  
            "OFFSET", "TRIM", "EXTEND", "FILLET", "CHAMFER", "SCALE",
            // Annotation Commands
            "DIMENSION", "TEXT", "MTEXT", "LEADER", "TABLE",
            // Layer Commands
            "LAYER", "LAYCUR", "LAYISO", "LAYUNISO",
            // View Commands
            "ZOOM", "PAN", "REGEN", "REDRAW",
            // Additional useful commands
            "HATCH", "COPY", "MOVE", "ROTATE", "MIRROR", "ARRAY"
        };


        [CommandMethod("pokretanje")]
        public void KeywordTranslation()
        {
            Prompt_is_on_main = true;
            main_commands_values = new List<commands_data>();

            foreach (string command in startupCommands)
            { check_and_add_command(command); }



            KeyboardFilterEnabled = true;
            //alternativeCommands = false;


            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var ed = doc.Editor;

            if (_window == null)
            {
                _window = new MainWindow(this);
                _window.Show();
                Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
            }

            doc.CommandWillStart += Doc_CommandWillStart;
            doc.CommandEnded += Doc_CommandEnded;
            doc.CommandCancelled += Doc_CommandCancelled;
            doc.CommandFailed += Doc_CommandFailed; ;

            // ed.LeavingQuiescentState += Ed_LeavingQuiescentState;//zapoceli smo neku naredbu
            ed.EnteringQuiescentState += Ed_EnteringQuiescentState;

            ed.PromptingForPoint += OnPromptingForPoint;
            ed.PromptingForDistance += OnPromptingForDistance;
            ed.PromptingForInteger += OnPromptingForInteger;
            ed.PromptingForDouble += OnPromptingForDouble;

            ed.PromptingForString += OnPromptingForString;
            ed.PromptingForKeyword += OnPromptingForKeyword;

            // We'll also watch keystrokes, to see when global keywords
            // are entered
            numKeyStrokes = new List<string>();
            Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage += OnPreTranslateMessage;

            // We need to turn off dynamic input: we'll reset the value
            // when we unload or in KWSX

            //_dynmode = (short)Application.GetSystemVariable("DYNMODE");
            //if (_dynmode != 0)
            //{
            //Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("DYNMODE", 0);
            //    ed.WriteMessage(
            //      "\nDynamic input has been disabled and can be re-enabled"
            //      + " by the KWSX command."
            //    );
            //}
            //ed.WriteMessage(
            //  "\nGlobal keyword dialog enabled. Run KWSX to turn it off."
            //);
        }


        #region pokrenili smo naredbu i vise nismo na main 
        private void Doc_CommandWillStart(object sender, CommandEventArgs e)
        {
            active_main_command = e.GlobalCommandName;
            check_and_add_command(e.GlobalCommandName);

            Prompt_is_on_main = false;
        }
        #endregion

        #region zavrsili smo naredbu i na main promptu smo
        private void Doc_CommandEnded(object sender, CommandEventArgs e)                                                    //ode nesto fali
        {
            Prompt_is_on_main = true;
        }

        private void Doc_CommandFailed(object sender, CommandEventArgs e)
        {
            Prompt_is_on_main = true;
        }

        private void Doc_CommandCancelled(object sender, CommandEventArgs e)
        {
            Prompt_is_on_main = true;
        }
        #endregion


        #region prompt for keywords string double integer distance
        private void OnPromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            DisplayKeywordsNOVO(e.Options.Keywords);
        }

        private void OnPromptingForString(object sender, PromptStringOptionsEventArgs e)
        {
            DisplayKeywordsNOVO(e.Options.Keywords);
        }

        private void OnPromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            DisplayKeywordsNOVO(e.Options.Keywords);
        }

        private void OnPromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            DisplayKeywordsNOVO(e.Options.Keywords);
        }

        private void OnPromptingForDistance(object sender, PromptDistanceOptionsEventArgs e)
        {
            DisplayKeywordsNOVO(e.Options.Keywords);
        }

        void OnPromptingForPoint(object sender, PromptPointOptionsEventArgs e)          //TBD       //prikazuje sejvane distance
        {
            //_window.ShowDistancePresets();
            //keyboardFilterEnabled = true;
        }

        #endregion
        //keywordsNOVO i keywordsNOVO2 treba spojiti u jednu naredbu kako bi ovisno o botunu ispisalo brojeve ili keywordse
        private void DisplayKeywordsNOVO(KeywordCollection kws)
        {
            if (alternativeCommands == true)
            {
                window_display_data.Clear();
                if (kws.Count != 0)
                {
                    foreach (Keyword kw in kws)
                    {
                        if (kw.Enabled && kw.Visible && kw.GlobalName != "dummy")
                        {
                            window_display_data.Add(kw.LocalName);
                        }
                    }
                }
            }
            else
            {
                sendNumbersToWindow_display_data();
            }
        }

        private void Ed_EnteringQuiescentState(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }

        public void check_and_add_command(string active_command)   //vidi dal' ti triba ode atribut
        {
            //provjeri da li vec u main_command_values postoji komanda; ako ne postoji; dodaj novu; ako postoji; povecaj counter
            int index = main_commands_values.FindIndex(i => i.command == active_command);

            if (index < 0)  //ako nije u listi, dodaj u listu
            {
                main_commands_values.Add(new commands_data(active_command));
            }
            else  //ako je vec u listi, povecaj counter za tu naredbu
            {
                main_commands_values[index].increase_counter();
            }

            //napomena; u eventu da je komanda zavrsena; dodaj zadnji uneseni broj  add_last_num_value_to_last_active_command()
        }                    //provjeravamo da li je komanda vec dodana - ako nije dodajemo je u listu ELSE povecajemo joj counter

        public void CheckAndAdd_num_to_command(double number)                         // DODAJE ZADJNJI BROJ ZADNJOJ NAREDBI
        {
            double used_number = number;
            int index = main_commands_values.FindIndex(i => i.command == active_main_command);
            var currentCommand = main_commands_values[index];
            int numIndex = currentCommand.added_numbers.FindIndex(i => i.number == used_number);
            if (numIndex < 0)
            {   //ako broj nije u listi dodamo ga
                main_commands_values[index].added_numbers.Add(new dimensionData(used_number));
            }
            else
            {   //ako je broj u listi povecamo mu counter
                main_commands_values[index].added_numbers[numIndex].increase_counter();
            }

        }

    }
}
