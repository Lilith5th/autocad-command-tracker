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
    public partial class MyCommands : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the PropertyChange event for the property specified
        /// </summary>
        /// <param name="propertyName">Property name to update. Is case-sensitive.</param>
        public virtual void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

        private MainWindow _window = null;
        private double last_entered_number { get; set; }


        private ObservableCollection<string> _window_display_data;
        public ObservableCollection<string> window_display_data                                             //OUTPUT ZA WPF
        {
            get { return _window_display_data; }
            set
            {
                _window_display_data = value;
                RaisePropertyChanged("window_display_data");
            }
        }
        private List<commands_data> main_commands_values
        {
            get;
            set;
        }                                                                                                   //GLAVNA BAZA PODATAKA
    }
}