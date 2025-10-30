using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2017_test_binding
{
    /// <summary>
    /// Interaction logic for MainWindows.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public static MyCommands myModel { get; set; }
        

        public MainWindow(MyCommands model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
                
            myModel = model;
            this.DataContext = myModel;
            InitializeComponent();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
