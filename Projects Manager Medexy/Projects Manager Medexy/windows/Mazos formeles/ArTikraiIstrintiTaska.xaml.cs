using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for ArTikraiIstrintiTaska.xaml
    /// </summary>
    public partial class ArTikraiIstrintiTaska : Window
    {
        public ArTikraiIstrintiTaska()
        {
            InitializeComponent();
        }

        #region variables

        public int TaskId;
        public int ProjectId;

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string deleteStr = "delete from tasks where id = @taks_id and project_id = @project_id";
            NpgsqlCommand deleteCmd = new NpgsqlCommand(deleteStr, log_in_form.prisijungimas);
            deleteCmd.Parameters.AddWithValue("@task_id", TaskId);
            deleteCmd.Parameters.AddWithValue("@project_id", ProjectId);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            deleteCmd.ExecuteNonQuery();
        }
    }
}
