using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DataRowView = System.Data.DataRowView;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for add_project.xaml
    /// </summary>
    ///
    public partial class add_project : Window
    {
        public add_project()
        {
            InitializeComponent();
        }

        private void myMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUsersDg();
            UpdateTaskUsersDg();
            UpdateTaskAtsakingasDg();

            TaskStartDate = DateTime.Today;
            TaskEndDate = DateTime.Today;
            ProjectStartDate = DateTime.Today;
            ProjectEndDate = DateTime.Today;

            TaskKomentaras.Document.Blocks.Clear();
            TaskKomentaras.Document.Blocks.Add(new Paragraph(new Run("Komentaras")));
        }

        #region When stuff changes (date pickers and datagrids)

        private void TaskAtsakingasDg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskAtsakingasDg.SelectedItems.Count > 0)
            {
                TaksResponsibleSelected = (DataRowView) TaskAtsakingasDg.SelectedItems[0];
            }
        }

        private void TasksDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksDatagrid.SelectedItems.Count > 0)
            {
                TasksDgSelected = TasksDatagrid.SelectedItems[0];
            }
        }

        private void TaskStartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskStartDate = (DateTime) TaskStartDatePicker.SelectedDate;
        }

        private void TaskEndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskEndDate = (DateTime) TaskEndDatePicker.SelectedDate;
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectStartDate = (DateTime) StartDatePicker.SelectedDate;
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectEndDate = (DateTime) EndDatePicker.SelectedDate;
        }

        #endregion

        #region variables

        #region for display purposes

        private DataRowView TaksResponsibleSelected;
        private object TasksDgSelected;

        private DateTime TaskStartDate;
        private DateTime TaskEndDate;

        private DateTime ProjectStartDate;
        private DateTime ProjectEndDate;

        DataTable TasksDataGrid = new DataTable();

        #endregion

        #region for saving purposes

        private int CurrentProjectId;

        #endregion

        #endregion

        #region set scaling


        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue",
            typeof(double), typeof(add_project),
            new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            add_project mainWindow = o as add_project;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double) value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            add_project mainWindow = o as add_project;

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
            double yScale = ActualHeight / 840f;
            double xScale = ActualWidth / 1100f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double) OnCoerceScaleValue(myMainWindow, value);
        }

        #endregion

        #region drag_n_drop files

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (string file in files)
                {
                    MessageBox.Show(file);

                    //uplaod a file to ftp
                    //WebClient client = new WebClient();
                    //string localFilePath = "";
                    //client.UploadFile("ftp://ftpserver.com/target.zip", "STOR", localFilePath);
                }
            }
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region UpdateDgs

        private void UpdateUsersDg()
        {
            string slct_str = "select * from users where upper(username) like @search";

            if (added_users_dg.Items.Count == 0)
            {
                slct_str = "select * from users where upper(username) like @search";
            }

            else if (added_users_dg.Items.Count > 0
            ) // jeigu daugiau negu 0 uz kiekviena prided po "or name != @new_name
            {
                slct_str = "select * from users where ( ";
                foreach (DataRowView row in added_users_dg.Items)
                {
                    slct_str += "username != '" + row[1] + "' and ";
                }

                slct_str = slct_str.Remove(slct_str.Length - 5);
                slct_str += ") and upper(username) like @search";
            }

            NpgsqlCommand slctUsers = new NpgsqlCommand(slct_str, log_in_form.prisijungimas);
            string searchStr = searchTb.Text.ToUpper();
            if (searchStr == "IEŠKOTI")
            {
                searchStr = "";
            }

            slctUsers.Parameters.AddWithValue("@search", "%" + searchStr + "%");

            DataTable dt = new DataTable("users");

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctUsers))
            {
                da.Fill(dt);
            }

            users_dg.ItemsSource = dt.AsDataView();
        }

        private void UpdateTaskAtsakingasDg()
        {
            string SlctCmdStr = "Select * from users where upper(username) like @search";
            NpgsqlCommand SlctCmd = new NpgsqlCommand(SlctCmdStr, log_in_form.prisijungimas);
            string search = TaskAtsakingasSearchTb.Text.ToUpper();
            if (search == "IEŠKOTI")
            {
                search = "";
            }

            SlctCmd.Parameters.AddWithValue("@search", "%" + search + "%");
            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            DataTable dt = new DataTable();
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(SlctCmd))
            {
                da.Fill(dt);
            }

            TaskAtsakingasDg.ItemsSource = dt.AsDataView();
        }

        private void UpdateTaskUsersDg()
        {
            string slct_str = "select * from users where upper(username) like @search";

            if (TaskAddedUsersDg.Items.Count == 0)
            {
                slct_str = "select * from users where upper(username) like @search";
            }

            else if (TaskAddedUsersDg.Items.Count > 0
            ) // jeigu daugiau negu 0 uz kiekviena prided po "or name != @new_name
            {
                slct_str = "select * from users where ( ";
                foreach (DataRowView row in TaskAddedUsersDg.Items)
                {
                    slct_str += "username != '" + row[1] + "' and ";
                }

                slct_str = slct_str.Remove(slct_str.Length - 5);
                slct_str += ") and upper(username) like @search";
            }

            NpgsqlCommand slctUsers = new NpgsqlCommand(slct_str, log_in_form.prisijungimas);
            string searchStr = TaskInvolvedSearchTb.Text.ToUpper();
            if (searchStr == "IEŠKOTI")
            {
                searchStr = "";
            }

            slctUsers.Parameters.AddWithValue("@search", "%" + searchStr + "%");

            DataTable dt = new DataTable("users");

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctUsers))
            {
                da.Fill(dt);
            }

            TaskUsersDg.ItemsSource = dt.AsDataView();
        }

        #endregion

        #region Text Boxes

        private void pavTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (pavTb.Text == "Pavadinimas")
            {
                pavTb.Text = "";
            }
        }

        private void TaskPavTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskPavTb.Text == "Pavadinimas")
            {
                TaskPavTb.Text = "";
            }
        }

        private void searchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTb.Text == "Ieškoti")
            {
                searchTb.Text = "";
            }
        }

        private void TaskAtsakingasSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskAtsakingasSearchTb.Text == "Ieškoti")
            {
                TaskAtsakingasSearchTb.Text = "";
            }
        }

        private void TaskAtsakingasSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTaskAtsakingasDg();
        }

        private void TaskInvolvedSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskInvolvedSearchTb.Text == "Ieškoti")
            {
                TaskInvolvedSearchTb.Text = "";
            }
        }

        private void searchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsersDg();
        }

        private void TaskInvolvedSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTaskUsersDg();
        }

        private void TaskKomentaras_GotFocus(object sender, RoutedEventArgs e)
        {
            string comment_string =
                new TextRange(this.TaskKomentaras.Document.ContentStart, this.TaskKomentaras.Document.ContentEnd).Text;
            if (comment_string == "Komentaras\r\n")
            {
                TaskKomentaras.Document.Blocks.Clear();
            }
        }
        
        #endregion

        #region Buttons

        #region projects

        private void AddUserBtnClick(object sender, RoutedEventArgs e)
        {
            if (users_dg.SelectedItems.Count > 0)
            {
                foreach (var t in users_dg.SelectedItems)
                {
                    added_users_dg.Items.Add(t);
                }
            }

            UpdateUsersDg();
        }

        private void RemoveUserBtnClick(object sender, RoutedEventArgs e)
        {
            int i = added_users_dg.SelectedItems.Count;
            int j = 0;
            while (j < i)
            {
                added_users_dg.Items.Remove(added_users_dg.SelectedItems[0]);
                j++;
            }

            UpdateUsersDg();
        }

        private void clearSearchTb_Click(object sender, RoutedEventArgs e)
        {
            searchTb.Text = "";
        }

        #endregion

        #region Tasks

        private void AddTaskUsersBtnClick(object sender, RoutedEventArgs e)
        {
            if (TaskUsersDg.SelectedItems.Count > 0)
            {
                foreach (var t in TaskUsersDg.SelectedItems)
                {
                    TaskAddedUsersDg.Items.Add(t);
                }
            }

            UpdateTaskUsersDg();
        }

        private void RemoveTaskUsersBtnClick(object sender, RoutedEventArgs e)
        {
            int i = TaskAddedUsersDg.SelectedItems.Count;
            int j = 0;
            while (j < i)
            {
                TaskAddedUsersDg.Items.Remove(TaskAddedUsersDg.SelectedItems[0]);
                j++;
            }

            UpdateTaskUsersDg();
        }

        private void ClearTaskAtsakingasSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TaskAtsakingasSearchTb.Text = "";
        }

        private void ClearTaskInvolvedSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TaskInvolvedSearchTb.Text = "";
        }


        #endregion

        #endregion

        #region Double CLicks

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (users_dg.SelectedItems.Count > 0)
            {
                foreach (var t in users_dg.SelectedItems)
                {
                    added_users_dg.Items.Add(t);
                }
            }

            UpdateUsersDg();
        }

        private void Row_DoubleClickAdded(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            int i = added_users_dg.SelectedItems.Count;
            int j = 0;
            while (j < i)
            {
                added_users_dg.Items.Remove(added_users_dg.SelectedItems[0]);
                j++;
            }

            UpdateUsersDg();
        }

        private void TaskAddedUsersDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            int i = TaskAddedUsersDg.SelectedItems.Count;
            int j = 0;
            while (j < i)
            {
                TaskAddedUsersDg.Items.Remove(TaskAddedUsersDg.SelectedItems[0]);
                j++;
            }

            UpdateTaskUsersDg();

        }

        private void TaskUsersDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (TaskUsersDg.SelectedItems.Count > 0)
            {
                foreach (var t in TaskUsersDg.SelectedItems)
                {
                    TaskAddedUsersDg.Items.Add(t);
                }
            }

            UpdateTaskUsersDg();

        }

        #endregion

        #region Adding Tasks functions (And removing)

        private void AddTasksBtnClick(object sender, RoutedEventArgs e)
        {
            string responsible = "";
            if (TaskAtsakingasDg.SelectedItems.Count > 0)
            {
                responsible = TaksResponsibleSelected[1].ToString();
            }

            string involved = "";

            foreach (var row in TaskAddedUsersDg.Items)
            {
                DataRowView rw = (DataRowView) row;
                involved += rw[1] + ", ";
            }

            if (involved.Length > 2)
            {
                involved.Remove(involved.Length - 2);
            }

            string comment = "";

            string richText = new TextRange(TaskKomentaras.Document.ContentStart, TaskKomentaras.Document.ContentEnd)
                .Text;

            comment = richText;

            TasksDataGrid.Rows.Add();
            if (TasksDataGrid.Columns.Count == 0)
            {
                TasksDataGrid.Columns.Add("name");
                TasksDataGrid.Columns.Add("start_date");
                TasksDataGrid.Columns.Add("end_date");
                TasksDataGrid.Columns.Add("comment");
                TasksDataGrid.Columns.Add("responsible");
                TasksDataGrid.Columns.Add("involved");
                TasksDataGrid.Columns.Add("creator");
            }

            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][0] = TaskPavTb.Text;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][1] = TaskStartDate.ToString();
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][2] = TaskEndDate.ToString();
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][6] = log_in_form.prisijunges_vartotojas;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][4] = responsible;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][5] = involved;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][3] = comment;

            TasksDatagrid.ItemsSource = TasksDataGrid.AsDataView();

            //TasksDatagrid.Items.Add(new TasksDatagridRow()
            //{
            //    name = TaskPavTb.Text,
            //    start_date = TaskStartDate,
            //    end_date = TaskEndDate,
            //    creator = log_in_form.prisijunges_vartotojas,
            //    responsible = responsible,
            //    involved = involved,
            //    comment = comment,
            //});

            ResetTaskBoxes();
        }

        private void RemoveTasksBtnClick(object sender, RoutedEventArgs e)
        {
            if (TasksDatagrid.Items.Contains(TasksDgSelected))
            {
                TasksDatagrid.Items.Remove(TasksDgSelected);
            }
            else if (TasksDatagrid.Items.Count > 0)
            {
                TasksDatagrid.Items.RemoveAt(TasksDatagrid.Items.Count - 1);
            }
        }

        private void ResetTaskBoxes()
        {
            TaskPavTb.Text = "Pavadinimas";
            TaskStartDatePicker.SelectedDate = TaskEndDatePicker.SelectedDate;
            TaskKomentaras.Document.Blocks.Clear();
            TaskKomentaras.Document.Blocks.Add(new Paragraph(new Run("Komentaras")));
            TaskAddedUsersDg.Items.Clear();
            TaskAtsakingasSearchTb.Text = "Ieškoti";
            TaskInvolvedSearchTb.Text = "Ieškoti";

        }

        public struct TasksDatagridRow
        {
            public string name { get; set; }
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
            public string creator { get; set; }
            public string responsible { get; set; }
            public string involved { get; set; }
            public string comment { get; set; }
        }

        #endregion

        #region Saving Project

        private void SaveProjectBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    SaveProject();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("project save failed");
                    throw;
                }

                try
                {
                    SaveTasks();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("task save failed");
                    throw;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                throw;
            }

            UpdateTaskAtsakingasDg();
            UpdateUsersDg();
            UpdateTaskUsersDg();

            MessageBox.Show("Projektas pridėtas");
            this.Close();

            //string richText = new TextRange(komentarasTb.Document.ContentStart, komentarasTb.Document.ContentEnd).Text;
            //string lyrics = richText;
            //Singer sing = new Singer();
            //sing.sing_queen();

        }

        private void SaveProject()
        {
            string SaveProjectInsertCmdStr =
                "insert into projects (" +
                "id," +
                "name," +
                "start_date," +
                "end_date," +
                "creator," +
                "involved," +
                "comment," +
                "completed)" +
                " values " +
                "(@id, " +
                "@name, " +
                "@start_date, " +
                "@end_date, " +
                "@creator, " +
                "@involved, " +
                "@comment, " +
                "false)";

            NpgsqlCommand SaveProjectInsertCmd = new NpgsqlCommand(SaveProjectInsertCmdStr, log_in_form.prisijungimas);

            int id = GetMaxProjectId();

            SaveProjectInsertCmd.Parameters.AddWithValue("@id", id);
            SaveProjectInsertCmd.Parameters.AddWithValue("@name", pavTb.Text);
            SaveProjectInsertCmd.Parameters.AddWithValue("@start_date", Convert.ToDateTime(StartDatePicker.SelectedDate));
            SaveProjectInsertCmd.Parameters.AddWithValue("@end_date", Convert.ToDateTime(EndDatePicker.SelectedDate));
            SaveProjectInsertCmd.Parameters.AddWithValue("@creator", log_in_form.prisijunges_vartotojas);

            string involved = "";
            foreach (var row in added_users_dg.Items)
            {
                DataRowView rw = (DataRowView) row;
                involved += rw[1] + ", ";
            }

            if (involved.Length > 2)
            {
                involved.Remove(involved.Length - 2);
            }

            SaveProjectInsertCmd.Parameters.AddWithValue("@involved", involved);

            string comment = new TextRange(komentarasTb.Document.ContentStart, komentarasTb.Document.ContentEnd)
                .Text;

            SaveProjectInsertCmd.Parameters.AddWithValue("@comment", comment);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            try
            {
                SaveProjectInsertCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }

            CurrentProjectId = id;
        }

        private int GetMaxProjectId()
        {
            string GetMaxProjectId = "Select max(id) from projects";
            NpgsqlCommand GetMaxProjectIdCmd = new NpgsqlCommand(GetMaxProjectId, log_in_form.prisijungimas);
            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            string id = GetMaxProjectIdCmd.ExecuteScalar().ToString();
            int id_old = Convert.ToInt32(id);
            int id_new = id_old;
            id_new++;
            return id_new;
        }

        private int GetMaxTaskId()
        {
            string GetMaxProjectId = "Select max(id) from tasks";
            NpgsqlCommand GetMaxProjectIdCmd = new NpgsqlCommand(GetMaxProjectId, log_in_form.prisijungimas);
            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            string id = GetMaxProjectIdCmd.ExecuteScalar().ToString();
            int id_old = Convert.ToInt32(id);
            int id_new = id_old;
            id_new++;
            return id_new;
        }

        private void SaveTasks()
        {
            foreach (var row in TasksDatagrid.Items)
            {
                DataRowView drv = (DataRowView)row;

                string InsertTaskCmdStr = "insert into tasks values (" +
                                          "@id, " +
                                          "@name, " +
                                          "@start_date, " +
                                          "@end_date, " +
                                          "@project_id, " +
                                          "@creator, " +
                                          "@responsible, " +
                                          "@involved, " +
                                          "@comment, " +
                                          "false)";

                NpgsqlCommand InsertTaskCmd = new NpgsqlCommand(InsertTaskCmdStr, log_in_form.prisijungimas);

                int id = GetMaxTaskId();

                InsertTaskCmd.Parameters.AddWithValue("@id", id);
                InsertTaskCmd.Parameters.AddWithValue("@name", drv[0].ToString());
                InsertTaskCmd.Parameters.AddWithValue("@start_date", Convert.ToDateTime(drv[1].ToString()));
                InsertTaskCmd.Parameters.AddWithValue("@end_date", Convert.ToDateTime(drv[2].ToString()));
                InsertTaskCmd.Parameters.AddWithValue("@project_id", CurrentProjectId);
                InsertTaskCmd.Parameters.AddWithValue("@creator", log_in_form.prisijunges_vartotojas);
                InsertTaskCmd.Parameters.AddWithValue("@responsible", drv[4].ToString());
                InsertTaskCmd.Parameters.AddWithValue("@involved", drv[5].ToString());
                InsertTaskCmd.Parameters.AddWithValue("@comment", drv[3].ToString());

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                InsertTaskCmd.ExecuteNonQuery();
            }
        }

        #endregion

        private void TestFunction()
        {
            string test = "test";
            foreach (var VARIABLE in added_users_dg.Items)
            {
                string type = VARIABLE.GetType().ToString();
            }

            for (int i = 0; i < TasksDatagrid.Items.Count; i++)
            {
                MessageBox.Show(TasksDatagrid.Items[i].ToString());
            }

            foreach (var VARIABLE in users_dg.Items)
            {
                string type = VARIABLE.GetType().ToString();
            }
        }

        private void Close_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
