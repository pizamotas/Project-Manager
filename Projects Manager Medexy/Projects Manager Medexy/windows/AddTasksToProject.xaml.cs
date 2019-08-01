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
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for AddTasksToProject.xaml
    /// </summary>
    public partial class AddTasksToProject : Window
    {
        public AddTasksToProject()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInfoAtStart();
            UpdateAtsakingasDg();
            UpdateTasksDatagrid();
            StyleTasksDatagrid();
        }

        #region Variables

        #region Opening

        public int ProjectId;

        #endregion

        #region Operational

        private DataTable ProjectInvolvedUsers;

        private DataRowView taskLastSelected;

        private string atsakingasLastSelected;

        DataTable TasksDataGrid = new DataTable();

        private DateTime TaskStartDate;
        private DateTime TaskEndDate;

        private DataTable TasksDatagridItemSource;


        #endregion

        #endregion

        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue",
            typeof(double), typeof(AddTasksToProject),
            new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            AddTasksToProject mainWindow = o as AddTasksToProject;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            AddTasksToProject mainWindow = o as AddTasksToProject;

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
            double yScale = ActualHeight / 550f;
            double xScale = ActualWidth / 1080f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        #endregion

        void UpdateInfoAtStart()
        {
            string slctInfoStr = "Select * from projects where id = @id";
            NpgsqlCommand SlctInfoCmd = new NpgsqlCommand(slctInfoStr, log_in_form.prisijungimas);
            SlctInfoCmd.Parameters.AddWithValue("@id", ProjectId);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataReader dr = SlctInfoCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    

                }
                dr.Close();
            }
        }

        DataTable GetInvolvedUsersTable()
        {
            string listOfInvolved = "";

            string GetListStr = "select involved from projects where id = @id";
            NpgsqlCommand slctInvolvedCmd = new NpgsqlCommand(GetListStr, log_in_form.prisijungimas);
            slctInvolvedCmd.Parameters.AddWithValue("@id", ProjectId);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataReader dr = slctInvolvedCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    listOfInvolved = dr[0].ToString();
                }
                dr.Close();
            }

            string names = listOfInvolved;

            string[] filters = names
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();

                }

                using (var command = log_in_form.prisijungimas.CreateCommand())
                {
                    command.Connection = log_in_form.prisijungimas;

                    command.CommandText =
                        $@"select *
           from users
          where username in ({string.Join(", ", filters.Select((name, i) => $"@prm_Name{i}"))})";

                    for (int i = 0; i < filters.Length; ++i)
                        command.Parameters.AddWithValue($"@prm_Name{i}", filters[i]);
                    DataTable dt = new DataTable();


                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(command))
                    {
                        da.Fill(dt);
                    }
                    return dt;
                }
        }

        void UpdateAtsakingasDg()
        {
            TaskAtsakingasDg.ItemsSource = GetInvolvedUsersTable().AsDataView();
        }

        private void TasksDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksDatagrid.SelectedItems.Count > 0)
            {
                taskLastSelected = (DataRowView) TasksDatagrid.SelectedItems[0];
            }
        }

        private void TaskPavTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskPavTb.Text == "Pavadinimas")
            {
                TaskPavTb.Text = "";
            }
        }

        private void TaskAtsakingasDg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskAtsakingasDg.SelectedItems.Count > 0)
            {
                DataRowView atsakingasRow;
                atsakingasRow = (DataRowView) TaskAtsakingasDg.SelectedItems[0];
                atsakingasLastSelected = atsakingasRow[0].ToString();
            }
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

        private void AddTasksBtnClick(object sender, RoutedEventArgs e)
        {
            string responsible = "";
            if (TaskAtsakingasDg.SelectedItems.Count > 0)
            {
                DataRowView AtsakingasRow = (DataRowView) TaskAtsakingasDg.SelectedItems[0];
                responsible = AtsakingasRow[0].ToString();
            }

            string involved = "";

            foreach (var row in TaskAddedUsersDg.Items)
            {
                DataRowView rw = (DataRowView)row;
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
            if (TasksDataGrid.Columns.Count == 0)
            {
                TasksDataGrid.Columns.Add("name");
                TasksDataGrid.Columns.Add("start_date");
                TasksDataGrid.Columns.Add("end_date");
                TasksDataGrid.Columns.Add("comment");
                TasksDataGrid.Columns.Add("responsible");
                TasksDataGrid.Columns.Add("involved");
                TasksDataGrid.Columns.Add("creator");

                TasksDataGrid.Columns["start_date"].DataType = typeof(DateTime);
                TasksDataGrid.Columns["end_date"].DataType = typeof(DateTime);
            }


            TasksDataGrid.Rows.Add();
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][0] = TaskPavTb.Text;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][1] = TaskStartDate.ToString();
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][2] = TaskEndDate.ToString();
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][6] = log_in_form.prisijunges_vartotojas;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][4] = responsible;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][5] = involved;
            TasksDataGrid.Rows[TasksDataGrid.Rows.Count - 1][3] = comment;

            var NewRow = new NewRow
            {
                TasskPav = TaskPavTb.Text,
                start_date = TaskStartDate.ToString(),
                end_date = TaskEndDate.ToString(),
                creator = log_in_form.prisijunges_vartotojas,
                responsible = responsible,
                involvedd = involved,
                comment =  comment
        };

            TasksDatagridItemSource.Merge(TasksDataGrid);

            TasksDatagrid.ItemsSource = TasksDatagridItemSource.AsDataView();





            //TasksDatagrid.ItemsSource = TasksDataGrid.AsDataView();

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

        private void TaskStartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskStartDate = (DateTime)TaskStartDatePicker.SelectedDate;
        }

        private void TaskEndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskStartDate = (DateTime)TaskEndDatePicker.SelectedDate;
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

        private void ClearTaskAtsakingasSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TaskAtsakingasSearchTb.Text = "";
        }

        #region Double CLicks

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

        private void TaskInvolvedSearchTb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskInvolvedSearchTb.Text == "Ieškoti")
            {
                TaskInvolvedSearchTb.Text = "";
            }
        }

        private void TaskInvolvedSearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTaskUsersDg();
        }

        private void ClearTaskInvolvedSearchTbBtnClick(object sender, RoutedEventArgs e)
        {
            TaskInvolvedSearchTb.Text = "";

        }

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

        private void RemoveTasksBtnClick(object sender, RoutedEventArgs e)
        {
            ArTikraiIstrintiTaska tr = new ArTikraiIstrintiTaska();
            MessageBox.Show("Exception 66: Message.Show(ex.message)");
            //if (taskLastSelected == null)
            //{
            //    MessageBox.Show("Nepažymėtas joks veiksmas");
            //}
            //else
            //{
            //    tr.TaskId = (int)taskLastSelected[0];
            //    tr.ProjectId = (int)taskLastSelected[4];
            //}
        }

        void UpdateTasksDatagrid()
        {
            string slctTasksStr = "select * from tasks where project_id = @id";
            NpgsqlCommand slctTasksCmd = new NpgsqlCommand(slctTasksStr, log_in_form.prisijungimas);
            slctTasksCmd.Parameters.AddWithValue("@id", ProjectId);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            DataTable dt = new DataTable();

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slctTasksCmd))
            {
                da.Fill(dt);
            }
            if (TasksDatagridItemSource == null)
            {
                TasksDatagridItemSource = dt;
            }
            else
            {
                TasksDatagridItemSource.Merge(dt);
            }

            TasksDatagrid.ItemsSource = TasksDatagridItemSource.AsDataView();
        }

        private void SaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            string DeleteAllPreviousTasks = "delete from tasks where project_id = @id";
            NpgsqlCommand DltALlTasksCmd = new NpgsqlCommand(DeleteAllPreviousTasks, log_in_form.prisijungimas);
            DltALlTasksCmd.Parameters.AddWithValue("@id", ProjectId);

            int MaxId = 0;

            string GetMaxIdStr = "select max(id) from tasks";
            NpgsqlCommand GetMaxIdCmd = new NpgsqlCommand(GetMaxIdStr, log_in_form.prisijungimas);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataReader dr = GetMaxIdCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    MaxId = (int) dr[0];
                }
                dr.Close();
            }

            DltALlTasksCmd.ExecuteNonQuery();

            foreach (object row in TasksDatagrid.Items)
            {
                DataRowView dr = (DataRowView) row;

                MaxId++;

                string insertTaskStr = "Insert into tasks (" +
                                       "id, " +
                                       "name, " +
                                       "start_date, " +
                                       "end_date, " +
                                       "project_id, " +
                                       "creator, " +
                                       "responsible, " +
                                       "involved, " +
                                       "comment, " +
                                       "completed) values (" +
                                       "@id, " +
                                       "@name, " +
                                       "@start_date, " +
                                       "@end_date, " +
                                       "@project_id, " +
                                       "@creator, " +
                                       "@responsible, " +
                                       "@involved, " +
                                       "@comment, " +
                                       "false) ";

                NpgsqlCommand InsertTasksCmd = new NpgsqlCommand(insertTaskStr, log_in_form.prisijungimas);
                InsertTasksCmd.Parameters.AddWithValue("@id", MaxId);
                InsertTasksCmd.Parameters.AddWithValue("@name", dr[1]);
                InsertTasksCmd.Parameters.AddWithValue("@start_date", Convert.ToDateTime(dr[2]));
                InsertTasksCmd.Parameters.AddWithValue("@end_date", Convert.ToDateTime(dr[3]));
                InsertTasksCmd.Parameters.AddWithValue("@project_id", ProjectId);
                InsertTasksCmd.Parameters.AddWithValue("@creator", log_in_form.prisijunges_vartotojas);
                InsertTasksCmd.Parameters.AddWithValue("@responsible", dr[6]);
                InsertTasksCmd.Parameters.AddWithValue("@involved", dr[7]);
                InsertTasksCmd.Parameters.AddWithValue("@comment", dr[8]);

                if (log_in_form.prisijungimas.State != ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                InsertTasksCmd.ExecuteNonQuery();

                MessageBox.Show("Projektas atnaujintas.");

                this.Close();
            }
        }

        void StyleTasksDatagrid()
        {
            TasksDatagrid.ColumnWidth = 110;
            TasksDatagrid.Columns[1].Width = 70;
            TasksDatagrid.Columns[2].Width = 70;
            TasksDatagrid.Columns[3].Width = 370;
            TasksDatagrid.Columns[4].Width = 60;
            TasksDatagrid.Columns[5].Width = 60;


        }

        private void CloseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class NewRow
    {
        public string TasskPav { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string creator { get; set; }
        public string responsible { get; set; }
        public string involvedd { get; set; }
        public string comment { get; set; }
    }


}
