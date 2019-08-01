using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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
using Microsoft.Win32;
using Npgsql;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for project_window.xaml
    /// </summary>
    public partial class project_window : Window
    {
        public project_window()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //Todo: Drag and drop files test
        public ObservableCollection<string> Files
        {
            get
            {
                return _files;
            }
        }
        private ObservableCollection<string> _files = new ObservableCollection<string>();

        private void myMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateValuesOnOpening();
            UpdateProjectsCommentsDg();
            StyleDataGrids();
            UpdateInvolvedDg();
            UpdateProjectFilesDg();
        }

        private void PopulateValuesOnOpening()
        {
            string slct_cmd_str = "select * from projects where id = @id";
            NpgsqlCommand slct_cmd = new NpgsqlCommand(slct_cmd_str, log_in_form.prisijungimas);

            slct_cmd.Parameters.AddWithValue("@id", opening_project_id);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_cmd))
            {
                da.Fill(opening_project_information);
            }

            PavadinimasLabel.Content = opening_project_information.Rows[0][1].ToString();
            StartDatelabel.Content = opening_project_information.Rows[0][2].ToString();
            endDatalabel.Content = opening_project_information.Rows[0][3].ToString();
            CreatorLaabel.Content = opening_project_information.Rows[0][5].ToString();

            CommentTextBox.Document.Blocks.Clear();
            CommentTextBox.Document.Blocks.Add(new Paragraph(new Run(opening_project_information.Rows[0][6].ToString())));

            UpdateTasksDataGrid();
        }



        #region Variables

        #region Opening

        public int opening_project_id;

        #endregion

        #region Operational

        private DataRowView tasksLastSelected;

        DataTable opening_project_information = new DataTable();
        DataTable opening_project_tasks = new DataTable();
        DataTable opening_project_comments = new DataTable();
        DataTable selected_task_comments = new DataTable();
        DataTable project_filesDT = new DataTable();
        DataTable task_filesDT = new DataTable();

        #endregion

        #endregion
        
        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue",
            typeof(double), typeof(project_window),
            new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            project_window mainWindow = o as project_window;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double) value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            project_window mainWindow = o as project_window;

            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double) e.OldValue, (double) e.NewValue);
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
            get { return (double) GetValue(ScaleValueProperty); }
            set { SetValue(ScaleValueProperty, value); }
        }


        //apatinio grido on SizeChanged eventas, reikia patikrint kad butu geras referencas <----------------- komentaras
        private void Grid_SizeChanged(object sender, EventArgs e)

        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 790f;
            double xScale = ActualWidth / 1720f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double) OnCoerceScaleValue(myMainWindow, value);
        }

        #endregion

        #region DataGrids

        private void StyleDataGrids()
        {
            Tasks_Dg.ColumnWidth = 60.5;
            Tasks_Dg.Columns[1].Width = 60.5;
            Tasks_Dg.Columns[2].Width = 42.35;
            Tasks_Dg.Columns[3].Width = 42.35;
            Tasks_Dg.Columns[4].Width = 230;
            Tasks_Dg.Columns[5].Width = 42.35;
            Tasks_Dg.Columns[6].Width = 42.35;

            //TaskComentsDataGrid.ColumnWidth = 100;
            //TaskComentsDataGrid.Columns[0].Width = 90;
            //TaskComentsDataGrid.Columns[1].Width = 75;
            //TaskComentsDataGrid.Columns[2].Width = 403;

        }

        private void UpdateTaskCommentsDataGrid()
        {
            selected_task_comments.Clear();

            string slct_task_comments_str = "select * from task_comments where task_id = @id";
            NpgsqlCommand slct_task_comments_cmd = new NpgsqlCommand(slct_task_comments_str, log_in_form.prisijungimas);

            try
            {
            slct_task_comments_cmd.Parameters.AddWithValue("@id", Convert.ToInt32(tasksLastSelected[0].ToString()));
            }
            catch (Exception e)
            {
                MessageBox.Show("Nepasirinktas veiksmas");
                return;
            }



            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            selected_task_comments.Clear();
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_task_comments_cmd))
            {
                try
                {
                    da.Fill(selected_task_comments);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                }

            }

            TaskComentsDataGrid.ItemsSource = selected_task_comments.AsDataView();

        }

        private void UpdateTasksDataGrid()
        {
            string slct_project_tasks_str = "select * from tasks where project_id = @id and upper(name) like @search";
            NpgsqlCommand slct_project_tasks_cmd = new NpgsqlCommand(slct_project_tasks_str, log_in_form.prisijungimas);
            slct_project_tasks_cmd.Parameters.AddWithValue("@id", opening_project_id);
            string search_string = TasksSsearchBox.Text.ToUpper();
            if (search_string == "IEŠKOTI")
            {
                search_string = "";
            }
            slct_project_tasks_cmd.Parameters.AddWithValue("@search", "%" + search_string + "%");

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            opening_project_tasks.Clear();

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_project_tasks_cmd))
            {
                da.Fill(opening_project_tasks);
            }



            Tasks_Dg.ItemsSource = opening_project_tasks.AsDataView();

        }

        private void UpdateInvolvedDg()
        {
            string SelectInvolvevdSstr = "select involved from projects where id = @id";
            NpgsqlCommand SlctInvolvedCmd = new NpgsqlCommand(SelectInvolvevdSstr, log_in_form.prisijungimas);
            SlctInvolvedCmd.Parameters.AddWithValue("@id", opening_project_id);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            DataTable ProjectInvolved = new DataTable();

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctInvolvedCmd))
            {
                da.Fill(ProjectInvolved);
            }

            InvolvedDatagid.ItemsSource = ProjectInvolved.AsDataView();
        }

        private void UpdateProjectsCommentsDg()
        {
            string slct_project_comments_str = "select project_comments.*, projects.name as name from project_comments join projects on project_comments.project_id = projects.id where project_id = @id";
            NpgsqlCommand slct_project_comments_cmd =
                new NpgsqlCommand(slct_project_comments_str, log_in_form.prisijungimas);
            slct_project_comments_cmd.Parameters.AddWithValue("@id", opening_project_id);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_project_comments_cmd))
            {
                opening_project_comments.Clear();
                da.Fill(opening_project_comments);
            }

            CommentsDataGrid.ItemsSource = opening_project_comments.AsDataView();

        }

        private void Tasks_Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tasks_Dg.SelectedItems.Count > 0)
            {
                tasksLastSelected = (DataRowView)Tasks_Dg.SelectedItems[0];
                UpdateTaskCommentsDataGrid();
                UpdateTaskFilesDg();
            }
        }

        private void UpdateProjectFilesDg()
        {
            String SlctProjectFilesStr = "select * from uploaded_files where project_id = @id";
            NpgsqlCommand SlctProjectFilesCmd = new NpgsqlCommand(SlctProjectFilesStr, log_in_form.prisijungimas);
            SlctProjectFilesCmd.Parameters.AddWithValue("@id", opening_project_id);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjectFilesCmd))
            {
                project_filesDT.Clear();
                da.Fill(project_filesDT);
            }

            ProjectFilesDg.ItemsSource = project_filesDT.AsDataView();
        }

        void UpdateTaskFilesDg()
        {
            string SlctTaskFilesStr = "select * from uploaded_files where task_id = @id";
            NpgsqlCommand SlctTaskFilesCmd = new NpgsqlCommand(SlctTaskFilesStr, log_in_form.prisijungimas);
            if (tasksLastSelected != null)
            {
                SlctTaskFilesCmd.Parameters.AddWithValue("@id", Convert.ToInt32(tasksLastSelected[0].ToString()));
            }
            else
            {
                return;
            }

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
            UpdateProjectFilesDg();
;        }

        void UpdateAllDgs()
        {
            StyleDataGrids();
            UpdateTaskCommentsDataGrid();
            UpdateTasksDataGrid();
            UpdateInvolvedDg();
            UpdateProjectsCommentsDg();
            UpdateProjectFilesDg();
            UpdateTaskFilesDg();
        }


        #endregion

        #region TextBoxes

        private void TasksSsearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TasksSsearchBox.Text == "Ieškoti")
            {
                TasksSsearchBox.Text = "";
            }

        }

        private void TasksSsearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTasksDataGrid();
        }

        private void AddCommentTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AddCommentTb.Text == "Pridėti komentarą")
            {
                AddCommentTb.Text = "";
            }
        }

        #endregion

        private void AddTaskCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tasksLastSelected != null)
            {

                String NewComment = AddCommentTb.Text;

                int max_id = 0;

                string slct_max_id_str = "select max(id) from task_comments";
                NpgsqlCommand slct_max_id = new NpgsqlCommand(slct_max_id_str, log_in_form.prisijungimas);

                Connector co = new Connector();
                co.conn();

                using (slct_max_id)
                {
                    NpgsqlDataReader dr = slct_max_id.ExecuteReader();
                    while (dr.Read())
                    {
                        max_id = Int32.Parse(dr[0].ToString());
                    }

                    max_id++;
                    dr.Close();
                }

                if (max_id == 0)
                {
                    MessageBox.Show("failed getting id of last row");
                }

                string insertTaskCommentStr =
                    "insert into task_comments (id, project_id, task_id, creator_public, creator_private, creator_ip, date, comment) " +
                    "Values (@id, @project_id, @task_id, @creator_public, @creator_private, @creator_ip, @date, @comment)";

                NpgsqlCommand InsesrtTaskCommentCmd =
                    new NpgsqlCommand(insertTaskCommentStr, log_in_form.prisijungimas);

                InsesrtTaskCommentCmd.Parameters.AddWithValue("@id", max_id);
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@project_id", opening_project_id);
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@task_id", Convert.ToInt32(tasksLastSelected[0]));
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_public", log_in_form.prisijunges_vartotojas);
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_private", Environment.MachineName);
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_ip",
                    Dns.GetHostAddresses(Environment.MachineName)[0].ToString());
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@date", DateTime.Today.Date);
                InsesrtTaskCommentCmd.Parameters.AddWithValue("@comment", AddCommentTb.Text);

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                InsesrtTaskCommentCmd.ExecuteNonQuery();

                AddCommentTb.Text = "";
            }

            UpdateTaskCommentsDataGrid();
            UpdateTaskFilesDg();
        }

        private void Close_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddMoreTasksBtn_Click(object sender, RoutedEventArgs e)
        {
            AddTasksToProject a = new AddTasksToProject();
            a.ProjectId = opening_project_id;
            a.Show();
        }

        private void AddProjectCommentBtnClick(object sender, RoutedEventArgs e)
        {
            string slctMaxProjectCommentId = "select max(id) from project_comments";
            NpgsqlCommand slctMaxProjectCommentsIdCmd = new NpgsqlCommand(slctMaxProjectCommentId, log_in_form.prisijungimas);

            int max_id=0; //Project comments max id

            Connector co = new Connector();
            co.conn();

            using (slctMaxProjectCommentsIdCmd)
            {
                NpgsqlDataReader dr = slctMaxProjectCommentsIdCmd.ExecuteReader();
                while (dr.Read())
                {
                    max_id = Int32.Parse(dr[0].ToString());
                }

                max_id++;
                dr.Close();

            }

            if (max_id == 0)
            {
                MessageBox.Show("failed getting id of last row");
            }

            string insertTaskCommentStr =
                "insert into project_comments (id, project_id, creator_public, creator_private, creator_ip, date, comment) " +
                "Values (@id, @project_id, @creator_public, @creator_private, @creator_ip, @date, @comment)";

            NpgsqlCommand InsesrtTaskCommentCmd =
                new NpgsqlCommand(insertTaskCommentStr, log_in_form.prisijungimas);

            InsesrtTaskCommentCmd.Parameters.AddWithValue("@id", max_id);
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@project_id", opening_project_id);
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_public", log_in_form.prisijunges_vartotojas);
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_private", Environment.MachineName);
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@creator_ip",
                Dns.GetHostAddresses(Environment.MachineName)[0].ToString());
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@date", DateTime.Today.Date);
            InsesrtTaskCommentCmd.Parameters.AddWithValue("@comment", AddProjectCommentTb.Text);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            InsesrtTaskCommentCmd.ExecuteNonQuery();

            AddCommentTb.Text = "";

            UpdateProjectsCommentsDg();
            UpdateProjectFilesDg();
        }

        private void AddProjectCommentTb_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (AddProjectCommentTb.Text == "Pridėti komentarą")
            {
                AddProjectCommentTb.Text = "";
            }
        }

        void Tasks_DgDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            task_window t = new task_window();
            t.TaskID = (int)tasksLastSelected[0];
            t.ProjectID = (int)tasksLastSelected[4];
            t.Show();
        }

        private void AddCommentTb_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                var listbox = sender as ListBox;
                listbox.Background = new SolidColorBrush(Color.FromRgb(155, 155, 155));
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void AddCommentTb_Drop(object sender, DragEventArgs e)
        {
            if (tasksLastSelected != null )
            {
                FileDownloader fd = new FileDownloader();
                fd.UploadFileStep1(opening_project_id, Convert.ToInt32(tasksLastSelected[0].ToString()), e, true);
                UpdateTaskCommentsDataGrid();
                UpdateTaskFilesDg();
            }
            else
            {
                MessageBox.Show("Nepasirinktas veiksmas");
            }

            #region OldCode
            ////Get project name for location

            //string projectName = "";

            //string SlctProjectNameStr = "Select name from projects where id = @id";
            //NpgsqlCommand SlctProjectName = new NpgsqlCommand(SlctProjectNameStr, log_in_form.prisijungimas);
            //SlctProjectName.Parameters.AddWithValue("@id", opening_project_id);

            //if (log_in_form.prisijungimas.State != ConnectionState.Open)
            //{
            //    log_in_form.prisijungimas.Open();
            //}

            //using (NpgsqlDataReader dr = SlctProjectName.ExecuteReader())
            //{
            //    while (dr.Read())
            //    {
            //        projectName = dr[0].ToString();
            //    }
            //}

            //projectName = projectName + "/";

            ////Get Task name for location

            //string taskName = "";

            //string SlctTaskNameStr = "Select name from tasks where id = @id";
            //NpgsqlCommand SlctTasktName = new NpgsqlCommand(SlctTaskNameStr, log_in_form.prisijungimas);

            //try 
            //{
            //    SlctTasktName.Parameters.AddWithValue("@id", tasksLastSelected[0]);
            //}
            //catch (Exception exception)
            //{
            //    if (exception.HResult == -2147467261)
            //    {
            //        MessageBox.Show("Nepasirinktas veiksmas");
            //        return;
            //    }

            //}


            //if (log_in_form.prisijungimas.State != ConnectionState.Open)
            //{
            //    log_in_form.prisijungimas.Open();
            //}

            //using (NpgsqlDataReader dr = SlctTasktName.ExecuteReader())
            //{
            //    while (dr.Read())
            //    {
            //        taskName = dr[0].ToString();
            //    }
            //}

            //taskName = taskName + "/";

            //string location = projectName + taskName;


            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //    UploadFiles(files, location);
            //}
            #endregion
        }

        private void AddCommentTb_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void AddCommentTb_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
       
        private void AddProjectCommentTb_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                var listbox = sender as ListBox;
                listbox.Background = new SolidColorBrush(Color.FromRgb(155, 155, 155));
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void AddProjectCommentTb_Drop(object sender, DragEventArgs e)
        {
            FileDownloader fd = new FileDownloader();
            fd.UploadFileStep1(opening_project_id, 0, e, false);
            UpdateProjectsCommentsDg();
            UpdateProjectFilesDg();

            #region OldCode
            ////Get project name for location

            //string projectName = "";

            //string SlctProjectNameStr = "Select name from projects where id = @id";
            //NpgsqlCommand SlctProjectName = new NpgsqlCommand(SlctProjectNameStr, log_in_form.prisijungimas);
            //SlctProjectName.Parameters.AddWithValue("@id", opening_project_id);

            //if (log_in_form.prisijungimas.State != ConnectionState.Open)
            //{
            //    log_in_form.prisijungimas.Open();
            //}

            //using (NpgsqlDataReader dr = SlctProjectName.ExecuteReader())
            //{
            //    while (dr.Read())
            //    {
            //        projectName = dr[0].ToString();
            //    }
            //}

            //projectName = projectName + "/";

            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //    UploadFiles(files, projectName);
            //}
            #endregion
        }

        private void AddProjectCommentTb_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void AddProjectCommentTb_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDownloader fd = new FileDownloader();
            fd.DownloadFile(1);
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

        private void ProjectFilesDg_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (ProjectFilesDg.SelectedItems.Count > 0)
            {
                DataRowView TaskFileLAstSelected = (DataRowView)ProjectFilesDg.SelectedItems[0];
                int TaskLastSelectedId = Convert.ToInt32(TaskFileLAstSelected[0].ToString());

                FileDownloader fd = new FileDownloader();
                fd.DownloadFile(TaskLastSelectedId);
            }

        }
    }
}
