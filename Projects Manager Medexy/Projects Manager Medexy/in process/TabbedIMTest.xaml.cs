using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using DataGrid = System.Windows.Controls.DataGrid;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Reflection.Emit.Label;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for test13.xaml
    /// </summary>
    public partial class test13 : Window
    {
        public test13()
        {
            InitializeComponent();
        }

        #region Variables

        #region operational

        private String TabName = "";
        private List<string> ListOfOpenTabs;

        #endregion

        #region Opening

        public string project_or_task;
        public int project_task_id;

        #endregion


        #endregion

        #region set scaling

        //Komentaras ----------------------------------------------------------------------------------------------------->      typeof(log_in_form) = window pavadinimas solution explorer
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(test13), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));


        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {

            //log_in_form = window pavadinimas solution explorer   <----------------- komentaras
            test13 mainWindow = o as test13;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            //log_in_form = window pavadinimas solution explorer        <----------------- komentaras
            test13 mainWindow = o as test13;

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
        private void Grid_SizeChanged(object sender, EventArgs e)

        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 450f;
            double xScale = ActualWidth / 800f;
            double value = Math.Min(xScale, yScale);

            //MyMainWindow = window pavadinimas propertis, ne is solution explorer  <----------------- komentaras
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);

        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TabItem sample_tab = (TabItem)Tab_control_test.Items[0];

            AddNewTab();

            //NewTabPage tab1 = new NewTabPage();
            //tab1.grid.Children.Add(tab1.dg);
            ////tab1.grid.Children.Add(tab1.dg2);
            //tab1.new_tab.Content = tab1.grid;
            //Tab_control_test.Items.Add(tab1.new_tab);
            //Tab_control_test.Items[Tab_control_test.Items.Count - 1] = sample_tab;
        }

        private class NewTabPage
        {
            public TabItem new_tab = new TabItem()
            {
                Header = "New tab"
            };

            public DataGrid dg = new DataGrid()
            {
                Name = "Data_Grid",
                Width = 200,
                Height = 200,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                //Margin = new Thickness(10, 10, 10, 10)
            };

            public DataGrid dg2 = new DataGrid()
            {

                Name = "Data_Grid2",
                Width = 200,
                Height = 200,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                //Margin = new Thickness(10, 10, 10, 10)
            };

            public Grid grid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 10, 10, 10)
            };



        };

        private void AddNewTab()
        {
            TabName = TabNameBox.Text;

            TabItem newTab = new TabItem();
            Grid gr = new Grid();

            DataGrid dg1 = new DataGrid();
            dg1.VerticalAlignment = VerticalAlignment.Top;
            dg1.HorizontalAlignment = HorizontalAlignment.Left;
            dg1.Width = 300;
            dg1.Height = 200;
            //dg1.RenderTransformOrigin = new System.Windows.Point(10, 10);

            DataTable tb = new DataTable();
            DataGridRow dgr = new DataGridRow();
            tb.Columns.Add();
            tb.Rows.Add();
            dg1.ItemsSource = tb.AsDataView();

            gr.Children.Add(dg1);

            Button btn = new Button();
            btn.Content = "New Button";
            btn.Width = 100;
            btn.Height = 30;
            btn.Margin = new Thickness(10, 230, 10, 10);
            btn.VerticalAlignment = VerticalAlignment.Top;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.Click += (sender, args) =>
            {
                BtnOnClickEvent(btn, args, newTab, (TextBox) gr.Children[2]);
            };

            TextBox TxtBox = new TextBox();
            TxtBox.Name = "Text_Box";
            TxtBox.VerticalAlignment = VerticalAlignment.Top;
            TxtBox.HorizontalAlignment = HorizontalAlignment.Left;
            TxtBox.Margin = new Thickness(10, 280,10, 10);
            TxtBox.Width = 300;

            gr.Children.Add(TxtBox);

            gr.Children.Add(btn);

            gr.VerticalAlignment = VerticalAlignment.Top;
            gr.HorizontalAlignment = HorizontalAlignment.Left;

            newTab.Content = gr;
            newTab.Header = TabName;

            Label project_or_task_lbl = new Label();
            
            
            
            Tab_control_test.Items.Add(newTab);
        }

        private void button1_Click()
        {
            // Iterates throught all the tabs in the tab control.
            foreach (System.Windows.Forms.TabPage tab in Tab_control_test.Items)
            {
                TabPage tabPage = (TabPage)tab;

                // Iterates through all the controls (i.e. textboxes, buttons, labels, etc.) in the tab page.
                foreach (System.Windows.Forms.Control control in tabPage.Controls)
                {
                    System.Windows.MessageBox.Show(control.GetType().ToString());
                }
            }
        }

        public static void CopyPropertiesTo<T, TU>(T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                .Where(x => x.CanWrite)
                .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }

        public void BtnOnClickEvent(Button sender, EventArgs e, TabItem tab, TextBox txt)
        {
            MessageBox.Show(tab.Header.ToString());
            txt.Text = "";
        }

        public void StartUpdating()
        {
            new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    auto_update_dg();
                }
            ).Start();

            new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    auto_update_active_tab();
                }
            ).Start();
        }

        public void auto_update_dg()
        {

            Thread.Sleep(Properties.Settings.Default.AutoUpdateTimerSlow);
            auto_update_dg();
        }

        public void auto_update_active_tab()
        {
            Thread.Sleep(Properties.Settings.Default.AutoUpdateTimerFast);
            auto_update_active_tab();
        }

    }
}
