using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Npgsql;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for main_menu.xaml
    /// </summary>
    public partial class main_menu : Window
    {
        #region variables

        private DataRowView projectsLastSelected;
        private DataRowView tasksLastSelected;

        #endregion

        public log_in_form log;

        public main_menu()
        {
            InitializeComponent();
        }

        private void myMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProjectsDg();
            Update_task_dg();
            style_dg();

            test_btn.Visibility = Visibility.Hidden;
            test_btn_2.Visibility = Visibility.Hidden;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                auto_update_dg();
            }).Start();
        }

        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue",
            typeof(double), typeof(main_menu),
            new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                new CoerceValueCallback(OnCoerceScaleValue)));


        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            main_menu mainWindow = o as main_menu;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double) value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            main_menu mainWindow = o as main_menu;

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
            ScaleValue = (double) OnCoerceScaleValue(myMainWindow, value);

        }


        #endregion

        #region DataGrid Updates

        private void UpdateProjectsDg()
        {
            string SlctProjects_Str =
                "Select * from projects where completed = false and involved like @vartotojas and " +
                "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
            NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);


            string search_str = ProjectSearchTb.Text.ToUpper();
            if (search_str == "IEŠKOTI")
            {
                search_str = "";
            }

            SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + search_str + "%");
            SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + log_in_form.prisijunges_vartotojas + "%");

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

        private void auto_update_dg()
        {
            NpgsqlConnection conn = new NpgsqlConnection(log_in_form.prisijungimas.ConnectionString);

            string SlctProjects_Str = "Select * from projects where completed != true and (involved like @vartotojas" +
                                      " or creator like @vartotojas) and " +
                                      "(upper(name) like @search or upper(creator) like @search or upper(involved) like @search)";
            NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, conn);

            string slct_tasks_str =
                "Select tasks.*, projects.name as project_name from tasks join projects on tasks.project_id = projects.id " +
                "where tasks.completed != true and (tasks.involved like @vartotojas or" +
                " responsible like @vartotojas or tasks.creator like @vartotojas) and " +
                "(upper(tasks.name) like @search or upper(tasks.creator) like @search or upper(tasks.involved) like @search)";
            NpgsqlCommand slct_tasks_cmd = new NpgsqlCommand(slct_tasks_str, conn);

            string search_str = "";
            string vartotojas = "";
            string search_tasks_str = "";
            Dispatcher.Invoke(() => { vartotojas = log_in_form.prisijunges_vartotojas; });


            conn.Open();


            while ((conn.State & ConnectionState.Open) != 0)
            {

                Dispatcher.Invoke(() => { search_str = ProjectSearchTb.Text.ToUpper(); });
                if (search_str == "IEŠKOTI")
                {
                    search_str = "";
                }

                SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + search_str + "%");
                SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + vartotojas + "%");

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctProjects_Cmd))
                {
                    DataTable dt = new DataTable("Projects");
                    da.Fill(dt);
                    if (Projects_Dg != null)
                    {
                        Dispatcher.Invoke(() => { Projects_Dg.ItemsSource = dt.AsDataView(); });
                    }
                }

                Dispatcher.Invoke(() => { search_tasks_str = TaskSearchTb.Text.ToUpper(); });
                if (search_tasks_str == "IEŠKOTI")
                {
                    search_tasks_str = "";
                }

                slct_tasks_cmd.Parameters.AddWithValue("@search", "%" + search_tasks_str + "%");
                slct_tasks_cmd.Parameters.AddWithValue("@vartotojas", "%" + vartotojas + "%");

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_tasks_cmd))
                {
                    DataTable dt = new DataTable("Projects");
                    da.Fill(dt);
                    if (Tasks_Dg != null)
                    {
                        Dispatcher.Invoke(() => { Tasks_Dg.ItemsSource = dt.AsDataView(); });
                    }
                }

                Thread.Sleep(5000);
            }

            auto_update_dg();
        }

        private void Update_task_dg()
        {
            string SlctProjects_Str =
                "Select tasks.*, projects.name as project_name from tasks join projects on tasks.project_id = projects.id " +
                "where tasks.completed != true and (tasks.involved like @vartotojas or" +
                " responsible like @vartotojas or tasks.creator like @vartotojas) and " +
                "(upper(tasks.name) like @search or upper(tasks.creator) like @search or upper(tasks.involved) like @search)";
            NpgsqlCommand SlctProjects_Cmd = new NpgsqlCommand(SlctProjects_Str, log_in_form.prisijungimas);

            string search_str = TaskSearchTb.Text.ToUpper();
            SlctProjects_Cmd.Parameters.AddWithValue("@search", "%" + search_str + "%");
            SlctProjects_Cmd.Parameters.AddWithValue("@vartotojas", "%" + log_in_form.prisijunges_vartotojas + "%");

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
            string slctTasksCommentsStr =
                "select task_comments.*, projects.name as project_name from task_comments join projects on task_comments.project_id = projects.id where task_comments.task_id = @id";
            NpgsqlCommand slctTaskCommentsCmd = new NpgsqlCommand(slctTasksCommentsStr, log_in_form.prisijungimas);

            slctTaskCommentsCmd.Parameters.AddWithValue("@id", Convert.ToInt32(tasksLastSelected[0].ToString()));

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

        #endregion

        #region Text boxes

        private void Search_Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProjectsDg();
        }

        private void TaskSearch_Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update_task_dg();
        }

        private void ProjectSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProjectSearchTb.Text == "Ieškoti")
            {
                ProjectSearchTb.Text = "";
            }
        }

        private void TaskSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskSearchTb.Text == "Ieškoti")
            {
                TaskSearchTb.Text = "";
            }
        }


        #endregion

        #region Buttons

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Projects p = new Projects();
            p.Show();
        }

        private void Close_btn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            test13 n = new test13();
            n.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            add_project p = new add_project();
            p.Show();
        }

        private void ClearSearchPro_Btn_Click(object sender, RoutedEventArgs e)
        {
            ProjectSearchTb.Text = "";
        }

        private void ClearTasksSearchBtnClick(object sender, RoutedEventArgs e)
        {
            TaskSearchTb.Text = "";
        }

        #endregion

        #region DataGrid Logic and design

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

        private void Projects_Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Projects_Dg.SelectedItems.Count > 0)
            {
                projectsLastSelected = (DataRowView) Projects_Dg.SelectedItems[0];
                UpdateProjectCommentsDatagrid();
            }
        }

        private void Tasks_Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tasks_Dg.SelectedItems.Count > 0)
            {
                tasksLastSelected = (DataRowView) Tasks_Dg.SelectedItems[0];
                UpdateTaskComments();
            }
        }

        private void ProjectsDGDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            project_window n = new project_window();
            n.opening_project_id = (int) projectsLastSelected[0];
            n.Show();

        }

        #endregion

        private void UpdateProjectCommentsDatagrid()
        {
            try
            {


                if (projectsLastSelected != null)
                {
                    string slctProjectCommentsCmdStr =
                        "select project_comments.*, projects.* from project_comments join projects on " +
                        "projects.id = project_comments.project_id where project_id = @id and " +
                        "(upper(project_comments.comment) like @search " +
                        "or upper(project_comments.creator_public) like @search) ";

                    NpgsqlCommand slctProjectCommentsCmd =
                        new NpgsqlCommand(slctProjectCommentsCmdStr, log_in_form.prisijungimas);

                    slctProjectCommentsCmd.Parameters.AddWithValue("@id", Convert.ToInt32(projectsLastSelected[0].ToString()));

                    string search_str = ProjectCommentSearchTb.Text.ToUpper();
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }

        }

        private void ProjectCommentSearchTbGotFocus(object sender, RoutedEventArgs e)
        {
            if (ProjectCommentSearchTb.Text == "Ieškoti")
            {
                ProjectCommentSearchTb.Text = "";
            }
        }

        private void ClearProjectCommentSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            ProjectCommentSearchTb.Text = "";
        }

        private void ProjectCommentSearchTbTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProjectCommentsDatagrid();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            project_window n = new project_window();
            n.Show();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            test tst = new test();
            tst.Show();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            test13 tabbed_im = new test13();
            tabbed_im.Show();
        }


        void Tasks_DgDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            task_window t = new task_window();
            t.TaskID = (int) tasksLastSelected[0];
            t.ProjectID = (int) tasksLastSelected[4];
            t.Show();
        }
    }
}
