using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows.Forms;

using System.Collections.ObjectModel;
using System.ComponentModel;


namespace _2017_test_binding
{

    #region my class types
    public class dimensionData
    {
        #region constructor
        public dimensionData()
        {
        }

        public dimensionData(double num)
        {
            number = num;
            counter = 0;
        }
        #endregion

        #region public properties
        public double number
        { get; set; }

        public int counter
        { get; set; }
        #endregion

        #region additional methods
        public void increase_counter()
        {
            counter++;
        }
        #endregion
    }

    public class commands_data : INotifyPropertyChanged
    {
        #region constructor
        public commands_data(string current_command)
        {
            command = current_command;
            counter = 0;
            added_numbers = new List<dimensionData>();

            //default Values
            added_numbers.Add(new dimensionData(1));
            added_numbers.Add(new dimensionData(10));
            added_numbers.Add(new dimensionData(100));
            added_numbers.Add(new dimensionData(1000));
        }
        #endregion

        #region private properties
        private string _command;
        private int _counter;
        private List<dimensionData> _added_numbers;
        #endregion

        #region public properties
        public string command   //OVDE SEJVAMO SVE KOMANDE-NEOGRANICEN BROJ
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
                this.OnPropertyChanged("Command");
            }
        }

        public int counter
        {
            get
            {
                return this._counter;
            }
            set
            {
                this._counter = value;
                this.OnPropertyChanged("Command counter");
            }
        }
        public List<dimensionData> added_numbers //OVDE SEJVAMO SVE BROJEVE KOJE SMO KORISTILI
        {
            get
            {
                return this._added_numbers;
            }
            set
            {
                this._added_numbers = value;
                this.OnPropertyChanged("Value");
            }
        }
        #endregion

        #region additional methods
        public void increase_counter()
        {
            counter++;
        }
        #endregion

        #region inotifyProperty part
        protected void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


    }
    #endregion 



    public partial class MyCommands
    {
        #region fields and properties
        private bool _prompt_is_on_main = true;
        private bool _alternativeCommands = false;
        private string _active_main_command;

        private bool Prompt_is_on_main
        {
            get
            {
                return _prompt_is_on_main;
            }
            set
            {
                _prompt_is_on_main = value;
                refreshMainWindow();
            }
        }
        private bool alternativeCommands
        {
            get
            {
                return _alternativeCommands;
            }
            set
            {
                _alternativeCommands = value;

            }
        }
        private string active_main_command
        {
            get { return _active_main_command; }
            set
            {
                _active_main_command = value;
                //commands_data curCommand = main_commands_values.Find(o => o.command == _active_main_command);
                // sendNumbersToWindow_display_data(curCommand);                                           // SALJE BROJEVE ZA TRENUTNU NAREDBU U PROZOR
            }
        }                           //naredba koja se trenutno izvrsava

        #endregion


        #region ovde se sve salje na main window
        private void refreshMainWindow()
        {
            if (main_commands_values != null)
            {
                if (Prompt_is_on_main == true)
                {
                    sendMain_commands_ToWindow_display_data();
                }
                else
                if (alternativeCommands == false)
                {
                    sendNumbersToWindow_display_data();
                }

            }
        }                                                             //ovo smo ugasili mozda bi trebalo izbrisat

        private void sendNumbersToWindow_display_data()                                                  //pribacuje listu u window_display_data
        {
            commands_data curCommand = main_commands_values.Find(i => i.command == active_main_command);
            List<dimensionData> curNumbers = curCommand.added_numbers;
            
            List<dimensionData> sortedNumbers = curNumbers.OrderByDescending(o => o.counter).ToList();              //SORTIRA BROJEVE
            List<string> listForDisplay = sortedNumbers.Select(x => x.number.ToString()).ToList();         //converta to string property i stavlja ga u novu listu
            window_display_data = new ObservableCollection<string>(listForDisplay);
        }

        private void sendMain_commands_ToWindow_display_data()                                           //pribacuje listu u window_display_data
        {
            List<commands_data> sortedList = main_commands_values.OrderByDescending(o => o.counter).ToList();     //SORTIRA NAREDBE PO BROJU KORISTENJA
            List<string> listForDisplay = sortedList.Select(x => x.command).ToList();                   //converta to string property i stavlja ga u novu listu
            window_display_data = new ObservableCollection<string>(listForDisplay);
        }
        #endregion
    }
}
