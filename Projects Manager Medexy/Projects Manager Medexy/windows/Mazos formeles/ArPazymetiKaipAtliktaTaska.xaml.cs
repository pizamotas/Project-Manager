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
    /// Interaction logic for ArPazymetiKaipAtliktaTaska.xaml
    /// </summary>
    public partial class ArPazymetiKaipAtliktaTaska : Window
    {
        public ArPazymetiKaipAtliktaTaska()
        {
            InitializeComponent();
        }

        #region variables

        public int task_id;
        public task_window tsk;


        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string updateStr = "update tasks set completed = true where id = @id";
            NpgsqlCommand UpdateCmd = new NpgsqlCommand(updateStr, log_in_form.prisijungimas);
            UpdateCmd.Parameters.AddWithValue("@id", task_id);
            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            UpdateCmd.ExecuteNonQuery();

            MessageBox.Show("Veiksmas pažymėtas kaip atliktas.");

            string GetProjectId =
                "select projects.id, *.tasks from projects join tasks on projects.id = tasks.project_id";

            tsk.Close();

            this.Close();
        }
    }
}
