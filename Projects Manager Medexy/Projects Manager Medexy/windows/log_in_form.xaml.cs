using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using Npgsql;


namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for log_in_form.xaml
    /// </summary>
    public partial class log_in_form : Window
    {
        public static bool is_admin = false;
        public static string prisijunges_vartotojas;

        static public NpgsqlConnection prisijungimas = new NpgsqlConnection(""); /* Redacted */

        public log_in_form()
        {
            InitializeComponent();
            Username_Tb.Focus();
        }



        private void logIn_btn_Click(object sender, RoutedEventArgs e)
        {

            bool user_pass = false;
            try
            {
                //prisijungia prie serverio
                if (prisijungimas.State != ConnectionState.Open)
                {
                    prisijungimas.Open();
                }

                //komanda patikrinti user + pass combo
                NpgsqlCommand LogIn = new NpgsqlCommand("select * from users where username = @vartotojas and password = @slaptazodis", prisijungimas);
                //komandos parametrai
                LogIn.Parameters.AddWithValue("@vartotojas", this.Username_Tb.Text);
                LogIn.Parameters.AddWithValue("@slaptazodis", this.Password_tb.Password);
                //data readeris su komanda
                NpgsqlDataReader rd = LogIn.ExecuteReader();
                //readerio rezultatai 
                if (rd.Read())
                {
                    if (rd["is_admin"].Equals(true))
                    {
                        is_admin = true;
                    }

                    //Pakeicia prisijungusi vartotoja
                    prisijunges_vartotojas = this.Username_Tb.Text;
                    //pakeiciua pass check i taip
                    user_pass = true;

                    //ijungia pagrindini meniu
                    prisijungimas.Close();
                    rd.Close();

                    main_menu main = new main_menu();
                    main.Show();
                    main.log = this;

                    this.Hide();

                }
                if (user_pass == false)
                {
                    //Pranesa jeigu nerado tokio vartotojo su slaptazodziu ir uzdaro connectiona
                    MessageBox.Show("Neteisingas slaptažodis");
                    prisijungimas.Close();
                    rd.Close();
                }
                rd.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepavyko prisijungti prie serverio." + Environment.NewLine + ex.Message);
                //MessageBox.Show(ex.Message);
            }

        }

        private void close_btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void exit()
        {
            this.Close();
        }

        #region set scaling

                //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
                public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(log_in_form), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));


        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            log_in_form mainWindow = o as log_in_form;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            log_in_form mainWindow = o as log_in_form;

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


        //apatinio grido on SizeChanged eventas, reikia patikrint kad butu geras referencas <----------------- komentaras
        private void MainGrid_SizeChanged(object sender, EventArgs e)

        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 320f;
            double xScale = ActualWidth / 340f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);

        }

        #endregion

        private void Username_Tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Password_tb.Focus();
            }
        }

        private void Password_tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                prisijungti_btn.Focus();
            }
        }

        private void NewUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddNewUser an = new AddNewUser();
            an.Show();
        }
    }

    public class Connector
    {
        public void conn()
        {
            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }
        }

        public void Connect()
        {
            try
            {
                Connector conn = new Connector();
                conn.conn2();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        public void conn2()
        {
            System.Threading.Thread.Sleep(10);
            conn3();
        }

        public void conn3()
        {
            System.Threading.Thread.Sleep(10);
            Connect();
        }
    }
}
