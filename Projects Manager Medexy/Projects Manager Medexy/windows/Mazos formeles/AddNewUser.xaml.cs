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
using System.Windows.Shapes;
using Npgsql;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for AddNewUser.xaml
    /// </summary>
    public partial class AddNewUser : Window
    {
        public AddNewUser()
        {
            InitializeComponent();
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Password1Tb.Text != Password2Tb.Text)
            {
                MessageBox.Show("Slaptažodžiai nesutampa");
                Password1Tb.Clear();
                Password2Tb.Clear();
                return;
            }


            //Get Max id
            int maxId = 0;
            string GetMaxIdStr = "Select max(id) from users";
            NpgsqlCommand GetMaxIdCmd = new NpgsqlCommand(GetMaxIdStr, log_in_form.prisijungimas);
            if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }
            using (NpgsqlDataReader dr = GetMaxIdCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    maxId = Convert.ToInt32(dr[0].ToString());
                }
                dr.Close();
            }
            maxId++;

            string InsertNewUserStr = "Insert into users (id, username, password, is_admin) " +
                "values (@id, @username, @password, false)";
            NpgsqlCommand InsertUser = new NpgsqlCommand(InsertNewUserStr, log_in_form.prisijungimas);
            InsertUser.Parameters.AddWithValue("@id", maxId);
            InsertUser.Parameters.AddWithValue("@username", UserNameTb.Text);
            InsertUser.Parameters.AddWithValue("@password", Password1Tb.Text);

            if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }
            InsertUser.ExecuteNonQuery();

            MessageBox.Show("Vartotojas: " + UserNameTb.Text + " pridėtas.");


        }
    }
}
