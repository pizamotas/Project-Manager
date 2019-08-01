using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Npgsql;
using Path = System.Windows.Shapes.Path;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class test : Window
    {
        public test()
        {
            InitializeComponent();
            this.AllowDrop = true;
        }

        #region old crap

        private void update_test_dg()
        {
            string slctTmp_str = "select * from temp";

            NpgsqlCommand slct_temp = new NpgsqlCommand(slctTmp_str, log_in_form.prisijungimas);

            if (log_in_form.prisijungimas.State != ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_temp))
            {
                DataTable dt = new DataTable("temp");
                da.Fill(dt);
                if (test_dg != null)
                {
                    test_dg.ItemsSource = dt.AsDataView();
                }
            }
        }

        private void start_updating()
        {
            string slctTmp_str = "select * from temp";
            NpgsqlConnection conn = new NpgsqlConnection(log_in_form.prisijungimas.ConnectionString);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            while (conn.State == ConnectionState.Open)
            {
                NpgsqlCommand slct_temp = new NpgsqlCommand(slctTmp_str, log_in_form.prisijungimas);
                DataTable dt = new DataTable("temp");

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(slct_temp))
                {
                    da.Fill(dt);

                }

                if (test_dg != null)
                {
                    this.Dispatcher.Invoke(() => { test_dg.ItemsSource = dt.AsDataView(); });
                }
                Thread.Sleep(500);
            }

            conn.Dispose();

            start_updating();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void test_dg_Drop(object sender, DragEventArgs e)
        {
            string fileName = "0";

            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) fileName = file;

            UploadFtpFile("/home/storage/Martynas/test", fileName, fileName);

        }

        public void UploadFtpFile(string folderName1, string fileName1, string absolute)
        {

            FtpWebRequest request;
            try
            {
                string folderName = folderName1;
                string fileName = fileName1;
                string absoluteFileName = absolute;

                request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", "" /* Redacted */,
                    folderName, absoluteFileName))) as FtpWebRequest;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Credentials = new NetworkCredential("" /* Redacted */,
                    "" /* Redacted */);
                request.ConnectionGroupName = "group";

                using (FileStream fs = File.OpenRead(fileName))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

