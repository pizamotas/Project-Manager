using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
    /// Interaction logic for task_window.xaml
    /// </summary>
    public partial class task_window : Window
    {
        public task_window()
        {
            InitializeComponent();
        }

        #region Variables

        #region Opening

        public int TaskID;
        public int ProjectID;

        #endregion

        #endregion

        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue",
            typeof(double), typeof(task_window),
            new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                new CoerceValueCallback(OnCoerceScaleValue)));


        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            task_window mainWindow = o as task_window;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            task_window mainWindow = o as task_window;

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
            get { return (double)GetValue(ScaleValueProperty); }
            set { SetValue(ScaleValueProperty, value); }
        }


        //apatinio grido on SizeChanged eventas, reikia patikrint kad butu geras referencas <----------------- komentaras
        private void Grid_SizeChanged_1(object sender, EventArgs e)

        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 400f;
            double xScale = ActualWidth / 1670f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);

        }


        #endregion


        private void AddCommentTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AddCommentTb.Text == "Pridėti komentarą")
            {
                AddCommentTb.Text = "";
            }
        }

        private void AddTaskCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string insertStr = "insert into task_comments (id, project_id, task_id, " +
                               "creator_public, creator_private, " +
                               "creator_ip, date, comment) " +
                               "values (" +
                               "@id, " +
                               "@project_id, " +
                               "@task_id, " +
                               "@creator_public, " +
                               "@creator_private, " +
                               "@creator_ip, " +
                               "@date, " +
                               "@comment)";

            NpgsqlCommand insertCmd = new NpgsqlCommand(insertStr, log_in_form.prisijungimas);

            //Get max comment id ++
            string slctMaxIdstr = "Select max(id) from task_comments";
            int max_id = 0;

            NpgsqlCommand slct_max_id = new NpgsqlCommand(slctMaxIdstr, log_in_form.prisijungimas);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            try
            {


                using (slct_max_id)
                {
                    NpgsqlDataReader dr = slct_max_id.ExecuteReader();
                    while (dr.Read())
                    {
                        max_id = Int32.Parse(dr[0].ToString());
                    }
                    dr.Close();
                }
            }
            catch (Exception exception)
            {
                return;
            }

            max_id++;

            insertCmd.Parameters.AddWithValue("@id", max_id);
            insertCmd.Parameters.AddWithValue("@project_id", ProjectID);
            insertCmd.Parameters.AddWithValue("@task_id", TaskID);
            insertCmd.Parameters.AddWithValue("@creator_public", log_in_form.prisijunges_vartotojas);
            insertCmd.Parameters.AddWithValue("@creator_private", Environment.MachineName);
            insertCmd.Parameters.AddWithValue("@creator_ip",
                Dns.GetHostAddresses(Environment.MachineName)[0].ToString());
            insertCmd.Parameters.AddWithValue("@date", DateTime.Today.Date);
            insertCmd.Parameters.AddWithValue("@comment", AddCommentTb.Text);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            insertCmd.ExecuteNonQuery();

            UpdateTaskCommentsDg();
            UpdateTaskFilesDg();
        }

        private void UpdateTaskCommentsDg()
        {
            string slctCommentsStr =
                "select * from task_comments where project_id = @project_id and task_id = @task_id";

            NpgsqlCommand slctCommentsCmd = new NpgsqlCommand(slctCommentsStr, log_in_form.prisijungimas);

            slctCommentsCmd.Parameters.AddWithValue("@task_id", TaskID);
            slctCommentsCmd.Parameters.AddWithValue("@project_id", ProjectID);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            DataTable TaskCommesntDT = new DataTable();

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctCommentsCmd))
            {
                da.Fill(TaskCommesntDT);
            }

            TaskComentsDataGrid.ItemsSource = TaskCommesntDT.AsDataView();
        }

        private void myMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Update all info on load
            string slctTaskInfoStr = "select * from tasks where id  = @id";
            NpgsqlCommand slctTaskInfoCmd = new NpgsqlCommand(slctTaskInfoStr, log_in_form.prisijungimas);
            slctTaskInfoCmd.Parameters.AddWithValue("@id", TaskID);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataReader dr = slctTaskInfoCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    PavadinimasLabel.Content = dr[1].ToString();
                    StartDatelabel.Content = dr[2].ToString();
                    endDatalabel.Content = dr[3].ToString();
                    CreatorLaabel.Content = dr[5].ToString();

                    CommentTextBox.Document.Blocks.Clear();
                    CommentTextBox.Document.Blocks.Add(new Paragraph(new Run(dr[8].ToString())));
                    InvolvedTextBox.Document.Blocks.Clear();
                    InvolvedTextBox.Document.Blocks.Add(new Paragraph(new Run(dr[7].ToString())));
                }
                dr.Close();
            }

            UpdateTaskCommentsDg();
            UpdateTaskFilesDg();

        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CompletedBtn_Click(object sender, RoutedEventArgs e)
        {
            ArPazymetiKaipAtliktaTaska frm = new ArPazymetiKaipAtliktaTaska();
            frm.task_id = TaskID;
            frm.tsk = this;
            frm.Show();
        }


        private void TaskComentsDataGrid_Drop(object sender, DragEventArgs e)
        {
            MessageBox.Show("test");
            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            //    // Assuming you have one file that you care about, pass it off to whatever
            //    // handling code you have defined.
            //    foreach (string file in files)
            //    {
            //        MessageBox.Show(file);

            //        //uplaod a file to ftp
            //        //WebClient client = new WebClient();
            //        //string localFilePath = "";
            //        //client.UploadFile("ftp://ftpserver.com/target.zip", "STOR", localFilePath);
            //    }
            //}


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                MessageBox.Show("test");

            }
        }

        private void TaskComentsDataGrid_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void TaskComentsDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("Test");
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.All;
                else
                    e.Effects = DragDropEffects.None;
            }
        }

        private void TaskFilesDg_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (TaskFilesDg.SelectedItems.Count > 0)
            {
                DataRowView TaskFileLAstSelected = (DataRowView)TaskFilesDg.SelectedItems[0];
                int TaskLastSelectedId = Convert.ToInt32(TaskFileLAstSelected[0].ToString());

                FileDownloader fd = new FileDownloader();
                fd.DownloadFile(TaskLastSelectedId);
            }

        }

        void UpdateTaskFilesDg()
        {
            DataTable task_filesDT = new DataTable();
            string SlctTaskFilesStr = "select * from uploaded_files where task_id = @id";
            NpgsqlCommand SlctTaskFilesCmd = new NpgsqlCommand(SlctTaskFilesStr, log_in_form.prisijungimas);
                SlctTaskFilesCmd.Parameters.AddWithValue("@id", Convert.ToInt32(TaskID));

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctTaskFilesCmd))
            {
                task_filesDT.Clear();
                da.Fill(task_filesDT);
            }
            TaskFilesDg.ItemsSource = task_filesDT.AsDataView();
        }
    }
}
