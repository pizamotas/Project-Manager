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

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void exit()
        {
            this.Close();
        }
        //carp

        #region scaling to res

        #region ScaleValue Depdency Property
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            //MainWindow = name of the window (tikras pav solution exploreryje)

            MainWindow mainWindow = o as MainWindow; 
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //MainWindow = name of the window (tikras pav)
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }
        #endregion

        private void MainGrid_SizeChanged(object sender, EventArgs e)
        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 250f;
            double xScale = ActualWidth / 200f;
            double value = Math.Min(xScale, yScale);
            //MainWindow1 = window name property value
            ScaleValue = (double)OnCoerceScaleValue(MainWindow1, value);
        }


        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }


        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            #region gets host name and ip address

            

            String strHostName;

            string strIPAddress;



            strHostName = System.Net.Dns.GetHostName();

            strIPAddress = System.Net.Dns.GetHostByName(strHostName).AddressList[0].ToString();


            MessageBox.Show("Host Name: " + strHostName + "; IP Address: " + strIPAddress);

            #endregion
        }



        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Skip the main window
            log_in_form n = new log_in_form();
            n.Show();
            this.Close();
        }
    }
}
