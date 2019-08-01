using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Npgsql;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for projects.xaml
    /// </summary>
    public partial class Projects : Window
    {

        #region Variables

        #region TempVariables

        private string selectedrowNum = "0";
        private DataRowView row1 = null;
        private DataRowView LastSelectedRow = null;

        private DataRowView LastSelectedProject;
        private DataRowView LastSelectedProjectComment;
        private DataRowView LastSelectedTask;
        private DataRowView LastSelectedTaskComment;

        #endregion

        #endregion

        public Projects()
        {
            InitializeComponent();
        }

        private void myMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProjectsDg();
           style_dg();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //updateDGScont();
            }).Start();
        }

        private void updateDGScont()
        {

            NpgsqlConnection conn = new NpgsqlConnection(log_in_form.prisijungimas.ConnectionString);
            conn.Open();

            while (conn.State == ConnectionState.Open)
            {
                int selected_row_count = 0;
                this.Dispatcher.Invoke(() => { selected_row_count = Projects_Dg.SelectedItems.Count; });
                if (selected_row_count > 0)
                {
                    this.Dispatcher.Invoke(() => { row1 = (DataRowView)Projects_Dg.SelectedItems[0]; });
                    selectedrowNum = row1[0].ToString();
                }

                string SlctProjects_Str_pro = "Select * from projects where (involved like @vartotojas or creator like @vartotojas) and " +
                                              "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
                ;
                NpgsqlCommand SlctProjects_Cmd_pro = new NpgsqlCommand(SlctProjects_Str_pro, log_in_form.prisijungimas);

                string searchTxt = "";
                this.Dispatcher.Invoke(() => { searchTxt = ProjectsSearchTb.Text; });
                string search_str = searchTxt.ToUpper();
                if (search_str == "IEŠKOTI")
                {
                    search_str = "";
                }
                SlctProjects_Cmd_pro.Parameters.AddWithValue("@search", "%" + search_str + "%");

                string Username = log_in_form.prisijunges_vartotojas;
                if (log_in_form.is_admin)
                {
                    Username = "";
                }

                SlctProjects_Cmd_pro.Parameters.AddWithValue("@vartotojas", "%" + Username  + "%");

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd_pro))
                {
                    DataTable dt = new DataTable("Projects");
                    da.Fill(dt);
                    if (Projects_Dg != null)
                    {
                        this.Dispatcher.Invoke(() => { Projects_Dg.ItemsSource = dt.AsDataView(); });
                    }
                }

                //Todo: Nuo cia viska patikrint ir sutvarkyt

            //    if (selected_row_count > 0)
            //    {
            //        string SlctProjects_Str = "Select * from tasks where completed != true and project_id = @id and " +
            //                                  "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
            //        NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);

            //        string task_search = "";

            //        this.Dispatcher.Invoke(() => { task_search = CompletedSearchTb.Text; });

            //        string tasksearch_str = task_search.ToUpper();
            //        SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + tasksearch_str + "%");
            //        SlctProjects_Cmd.Parameters.AddWithValue("@id", row1[0].ToString());

            //        if (log_in_form.prisijungimas.State != ConnectionState.Open)
            //        {
            //            log_in_form.prisijungimas.Open();
            //        }

            //        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd))
            //        {
            //            DataTable dt = new DataTable("Projects");
            //            da.Fill(dt);
            //            if (Tasks_Dg != null)
            //            {
            //                this.Dispatcher.Invoke(() => { Tasks_Dg.ItemsSource = dt.AsDataView(); });
            //            }
            //        }
            //    }
            //    else if (selectedrowNum != "0")
            //    {
            //        string SlctProjects_Str = "Select * from tasks where completed != true and project_id = @id and " +
            //                                  "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
            //        NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);

            //        string task_search = "";

            //        this.Dispatcher.Invoke(() => { task_search = ActiveSearchTb.Text; });

            //        string tasksearch_str = task_search.ToUpper();
            //        SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + tasksearch_str + "%");
            //        SlctProjects_Cmd.Parameters.AddWithValue("@id", row1[0].ToString());

            //        if (log_in_form.prisijungimas.State != ConnectionState.Open)
            //        {
            //            log_in_form.prisijungimas.Open();
            //        }

            //        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd))
            //        {
            //            DataTable dt = new DataTable("Projects");
            //            da.Fill(dt);
            //            if (Tasks_Dg != null)
            //            {
            //                this.Dispatcher.Invoke(() => { Tasks_Dg.ItemsSource = dt.AsDataView(); });
            //            }
            //        }
            //    }
            //    Thread.Sleep(5000);
            }
            //updateDGScont();
        }

        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Projects), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));


        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            Projects mainWindow = o as Projects;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            Projects mainWindow = o as Projects;

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
            double yScale = ActualHeight / 1080f;
            double xScale = ActualWidth / 1920f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        #endregion

        #region SearchBoxesEtc

        void ClearProjectsSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            ProjectsSearchTb.Text = "";
        }

        private void ClearProjectCommentsSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            ProjectCommentsSearchTb.Text = "";
        }

        private void ClearTasksSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TasksSearchTb.Text = "";
        }

        private void ClearTasksCommentsSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TaskCommentsSearchTb.Text = "";
        }

        private void ProjectsSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProjectsSearchTb.Text.ToLower() == "ieškoti")
            {
                ProjectsSearchTb.Text = "";
            }
        }

        private void ProjectCommentsSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProjectCommentsSearchTb.Text.ToLower() == "ieškoti")
            {
                ProjectCommentsSearchTb.Text = "";
            }

        }

        private void TasksSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TasksSearchTb.Text.ToLower() == "ieškoti")
            {
                TasksSearchTb.Text = "";
            }

        }

        private void TaskCommentsSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskCommentsSearchTb.Text.ToLower() == "ieškoti")
            {
                TaskCommentsSearchTb.Text = "";
            }

        }

        #endregion


        private void ProjectsSearchTb_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProjectsDg();
        }


        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Projects_Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Projects_Dg.SelectedItems.Count > 0)
            {
                LastSelectedProject = (DataRowView) Projects_Dg.SelectedItems[0];
            }
            Update_task_dg();
            UpdateProjectCommentsDatagrid();
        }

        private void Tasks_Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tasks_Dg.SelectedItems.Count > 0)
            {
                LastSelectedTask = (DataRowView) Tasks_Dg.SelectedItems[0];
            }
            UpdateTaskComments();
        }

        void UpdateProjectsDg()
        {
            string SlctProjects_Str =
                "Select * from projects where involved like @vartotojas and " +
                "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
            NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);


            string search_str = ProjectsSearchTb.Text.ToUpper();
            if (search_str == "IEŠKOTI")
            {
                search_str = "";
            }

            SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + search_str + "%");
            SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + log_in_form.prisijunges_vartotojas + "%");
            if (log_in_form.is_admin)
            {
                SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + "%");
            }

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd))
            {
                DataTable dt = new DataTable("Projects");
                da.Fill(dt);
                if (Projects_Dg != null)
                {
                    Projects_Dg.ItemsSource = dt.AsDataView();
                }
            }

        }

        private void style_dg()
        {
            Projects_Dg.ColumnWidth = 150;
            Projects_Dg.Columns[1].Width = 70;
            Projects_Dg.Columns[2].Width = 70;
            Projects_Dg.Columns[3].Width = 440;
            Projects_Dg.Columns[4].Width = 80;
            Projects_Dg.Columns[5].Width = 80;

            Tasks_Dg.ColumnWidth = 130;
            Tasks_Dg.Columns[1].Width = 100;
            Tasks_Dg.Columns[2].Width = 70;
            Tasks_Dg.Columns[3].Width = 70;
            Tasks_Dg.Columns[4].Width = 380;
            Tasks_Dg.Columns[5].Width = 70;
            Tasks_Dg.Columns[6].Width = 70;
        }


        private void Update_task_dg()
        {
            string SlctProjects_Str =
                "Select tasks.*, projects.name as project_name from tasks join projects on tasks.project_id = projects.id " +
                "where (tasks.involved like @vartotojas or" +
                " responsible like @vartotojas or tasks.creator like @vartotojas) and " +
                "(upper(tasks.name) like @search or upper(tasks.creator) like @search or upper(tasks.involved) like @search) " +
                "and project_id = @project_id";
            NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);

            string search_str = TasksSearchTb.Text.ToUpper();
            if (search_str != "IEŠKOTI")
            {
                SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + search_str + "%");
            }
            else
            {
                SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + "" + "%");
            }
            SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + log_in_form.prisijunges_vartotojas + "%");
            if (log_in_form.is_admin)
            {
                SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + "" +  "%");
            }
            if (LastSelectedProject == null)
            {
                return;
            }
            SlctProjects_Cmd.Parameters.AddWithValue("@project_id", Convert.ToInt32(LastSelectedProject[0].ToString()));


            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd))
            {
                DataTable dt = new DataTable("Projects");
                da.Fill(dt);
                if (Tasks_Dg != null)
                {
                    Tasks_Dg.ItemsSource = dt.AsDataView();
                }
            }
        }

        void UpdateTaskComments()
        {
            if (LastSelectedTask == null)
            {
                return;
            }
            string slctTasksCommentsStr =
                "select task_comments.*, projects.name as project_name from task_comments join projects on task_comments.project_id = projects.id where task_comments.task_id = @id";
            NpgsqlCommand slctTaskCommentsCmd = new NpgsqlCommand(slctTasksCommentsStr, log_in_form.prisijungimas);

            slctTaskCommentsCmd.Parameters.AddWithValue("@id", Convert.ToInt32(LastSelectedTask[0].ToString()));

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            DataTable dt = new DataTable();

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctTaskCommentsCmd))
            {
                da.Fill(dt);
            }
            TaskCommentsDatagrid.ItemsSource = dt.AsDataView();
        }

        private void UpdateProjectCommentsDatagrid()
        {
            if (LastSelectedProject != null)
            {
                string slctProjectCommentsCmdStr =
                    "select project_comments.*, projects.* from project_comments join projects on " +
                    "projects.id = project_comments.project_id where project_id = @id and " +
                    "(upper(project_comments.comment) like @search " +
                    "or upper(project_comments.creator_public) like @search) ";

                NpgsqlCommand slctProjectCommentsCmd =
                    new NpgsqlCommand(slctProjectCommentsCmdStr, log_in_form.prisijungimas);

                slctProjectCommentsCmd.Parameters.AddWithValue("@id", Convert.ToInt32(LastSelectedProject[0].ToString()));

                string search_str = ProjectCommentsSearchTb.Text.ToUpper();
                if (search_str == "IEŠKOTI")
                {
                    search_str = "";
                }

                slctProjectCommentsCmd.Parameters.AddWithValue("@search", "%" + search_str + "%");

                DataTable dt = new DataTable();

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctProjectCommentsCmd))
                {
                    da.Fill(dt);
                }

                ProjectCommentsDatagrid.ItemsSource = dt.AsDataView();
            }
        }

        private void ProjectsDGDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            project_window n = new project_window();
            n.opening_project_id = (int)LastSelectedProject[0];
            n.Show();

        }

        void Tasks_DgDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            task_window t = new task_window();
            t.TaskID = (int)LastSelectedTask[0];
            t.ProjectID = (int)LastSelectedTask[4];
            t.Show();
        }

        private void ProjectCommentsSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProjectCommentsDatagrid();
        }

        private void TasksSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update_task_dg();
        }

        private void TaskCommentsSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTaskComments();
        }
    }
}
