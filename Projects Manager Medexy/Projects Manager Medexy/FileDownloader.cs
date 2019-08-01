using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Npgsql;

namespace Projects_Manager_Medexy
{
    class FileDownloader
    {
        #region Variables for db updates

        #region uploaded files
        //comments show where data got from
        int uploaded_files_id = 0; //UploadFileStep2 after for each upload
        string uploaded_files_file_name = ""; //UploaadFileStep2 For each upload end
        string uploaded_files_project_name = ""; //UploadtFileStep1 SlctProjectName reader
        string uploaded_files_task_name = ""; //UploadFileStep1 SlctTaskName Reader
        string uploaded_files_full_path = ""; //UploadFileStep2 For each upload end
        string uploaded_files_file_extension = "";//UploadFileStep2 For each upload end
        int uploaded_files_project_id = 0; //UploadFileStep1 start
        int uploaded_files_task_id = 0; //UploadFileStep1 start

        #endregion

        #endregion


        public void DownloadFile(int FileId)
        {
            try
            {
                string FullPath = "";
                string FileName = "";
                string FileExtension = "";

                string SlctFileInfoStr = "Select * from uploaded_files where id = @id";
                NpgsqlCommand SlctFileInfoCmd = new NpgsqlCommand(SlctFileInfoStr, log_in_form.prisijungimas);
                SlctFileInfoCmd.Parameters.AddWithValue("@id", FileId);

                if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                using (NpgsqlDataReader dr = SlctFileInfoCmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        FullPath = dr["full_path"].ToString();
                        FileName = dr["file_name"].ToString();
                        FileExtension = "." + dr["file_extension"].ToString();
                    }
                    dr.Close();
                }

                Microsoft.Win32.SaveFileDialog sd = new Microsoft.Win32.SaveFileDialog();
                sd.FileName = FileName;
                sd.DefaultExt = FileExtension;

                if (sd.ShowDialog() == true)
                {
                    using (WebClient client = new WebClient())
                    {
                        string FullFilePath = FullPath;

                        client.Credentials = new NetworkCredential("", ""); //Redacted
                        client.DownloadFile(FullFilePath, sd.FileName);
                        MessageBox.Show("Siuntimas baigtas");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UploadFileStep1(int ProjectId, int TaskId, System.Windows.DragEventArgs e, bool TaskExists)
        {
            uploaded_files_project_id = ProjectId;
            uploaded_files_task_id = TaskId;

            
            string location = "";

            //Get project name for location
                        string projectName = "";

            string SlctProjectNameStr = "Select name from projects where id = @id";
            NpgsqlCommand SlctProjectName = new NpgsqlCommand(SlctProjectNameStr, log_in_form.prisijungimas);
            SlctProjectName.Parameters.AddWithValue("@id", ProjectId);

            if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }

            using (NpgsqlDataReader dr = SlctProjectName.ExecuteReader())
            {
                while (dr.Read())
                {
                    projectName = dr[0].ToString();
                    uploaded_files_project_name = projectName;
                }
                dr.Close();
            }

            projectName = projectName + "/";

            location = projectName;

            if (TaskExists)
            {
                //Get Task name for location
                string taskName = "";

                string SlctTaskNameStr = "Select name from tasks where id = @id";
                NpgsqlCommand SlctTasktName = new NpgsqlCommand(SlctTaskNameStr, log_in_form.prisijungimas);

                try
                {
                    SlctTasktName.Parameters.AddWithValue("@id", TaskId);
                }
                catch (Exception exception)
                {
                    throw;
                }


                if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                using (NpgsqlDataReader dr = SlctTasktName.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        taskName = dr[0].ToString();
                        uploaded_files_task_name = taskName;
                    }
                    dr.Close();
                }
                taskName = taskName + "/";

                location = projectName + taskName;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                UploadFileStep2(files, location, TaskExists);
            }

        }

        private void UploadFileStep2(string[] files, string location, bool IsTaskfile)
        {
            try
            {
                foreach (var file in files)
                {
                    using (WebClient client = new WebClient())
                    {
                        string FullFilePath = file.ToString();

                        string[] ShortName = FullFilePath.Split('\u005C');

                        client.Credentials = new NetworkCredential("", ""); //Redacted
                        client.UploadFile(
                            "" /* Redacted */  + location + ShortName[ShortName.Length - 1], 
                            WebRequestMethods.Ftp.UploadFile, file);

                        //getting pure file name for variables
                        string[] FileName = ShortName[ShortName.Length - 1].Split('.');
                        uploaded_files_file_name = ShortName[ShortName.Length - 1];
                        uploaded_files_full_path = "" /* Redacted */ + location + ShortName[ShortName.Length - 1];
                        uploaded_files_file_extension = FileName[1];
                    }
                }
                //Pridet komentarus i visus tables kuriem reikia zinot kaad ikleltas failas

                //get next id for insert
                string GetMaxIdStr = "select max(id) from uploaded_files";
                NpgsqlCommand GetMAxIdCmd = new NpgsqlCommand(GetMaxIdStr, log_in_form.prisijungimas);
                try
                {
                    if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                    {
                        log_in_form.prisijungimas.Open();
                    }
                    using (NpgsqlDataReader dr = GetMAxIdCmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            uploaded_files_id = Convert.ToInt32(dr[0].ToString());
                        }
                        dr.Close();
                    }
                }
                catch (Exception) //Catch exception if tables is empty
                {
                    throw;
                }
                uploaded_files_id++;

                string InsrtUploadedFilesStr = "insert into uploaded_files " +
                    "(id, file_name, project_name, task_name, date_uploaded, uploader_name, " +
                    "full_path, file_extension, project_id, task_id) " +
                    "values " +
                    "(@id, @file_name, @project_name, @task_name, @date_uploaded, @uploader_name, " +
                    "@full_path, @file_extension, @project_id, @task_id)";

                NpgsqlCommand InsertUploadedFilesCmd = new NpgsqlCommand(InsrtUploadedFilesStr, log_in_form.prisijungimas);

                InsertUploadedFilesCmd.Parameters.AddWithValue("@id", uploaded_files_id);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@file_name", uploaded_files_file_name);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@project_name", uploaded_files_project_name);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@task_name", uploaded_files_task_name);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@date_uploaded", Convert.ToDateTime(DateTime.Today));
                InsertUploadedFilesCmd.Parameters.AddWithValue("@uploader_name", log_in_form.prisijunges_vartotojas);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@full_path", uploaded_files_full_path);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@file_extension", uploaded_files_file_extension);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@project_id", uploaded_files_project_id);
                InsertUploadedFilesCmd.Parameters.AddWithValue("@task_id", uploaded_files_task_id);

                if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }
                InsertUploadedFilesCmd.ExecuteNonQuery();

                InsertCommentTPCommentsTables(IsTaskfile);
            }

            catch (Exception e)
            {
                //Invalid file name (create new directory /Project/Task)
                if (e.HResult == -2146233079)
                {
                    CreateDir(location, files, IsTaskfile);
                }

                else
                {
                    MessageBox.Show(e.Message);
                }

            }
        }

        private void CreateDir(string pathToCreate, string[] files, bool IsTaskFile)
        {
            //feeds back to uplaod files
            string[] Files = files;
            string location = pathToCreate;

            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;

            string[] subDirs = pathToCreate.Split('/');

            string currentDir = "" /* Redacted */;

            foreach (string subDir in subDirs)
            {
                try
                {
                    currentDir = currentDir + "/" + subDir;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(currentDir);
                    reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential("", ""); /* Redacted */
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    ftpStream = response.GetResponseStream();
                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    if (ex.HResult == -2146233079)
                    {
                        continue;
                        // continue the loop if project already exists
                    }
                    MessageBox.Show(ex.Message);
                }
            }
            UploadFileStep2(files, location, IsTaskFile);
        }

        private void InsertCommentTPCommentsTables(bool IsTaskFile)
        {
            int MaxId = 0;
            //Get Max Id from appropriate table
            string GetMaxIdStr = "";
            if (IsTaskFile)
            {
                GetMaxIdStr = "select max(id) from task_comments";
            }
            else
            {
                GetMaxIdStr = "select max(id) from project_comments";
            }

            NpgsqlCommand GetMaxIdCmd = new NpgsqlCommand(GetMaxIdStr, log_in_form.prisijungimas);


            //GetMaxIdCmd.Parameters.AddWithValue("@table", IsTaskFile ? "task_comments" : "project_comments");
            if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
            {
                log_in_form.prisijungimas.Open();
            }
            using (NpgsqlDataReader dr = GetMaxIdCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    MaxId = Convert.ToInt32(dr[0].ToString());
                }
                dr.Close();
            }
            MaxId++;

            if (IsTaskFile) //isskiria ar task commnets ar project comments
            {
                string InsertCommnetStr = "insert into task_comments (id, project_id, task_id, creator_public, date, comment) " +
                    "values (@id, @project_id, @task_id, @creator_public, @date, @comment)";
                NpgsqlCommand InsertCommentCmd = new NpgsqlCommand(InsertCommnetStr, log_in_form.prisijungimas);
                InsertCommentCmd.Parameters.AddWithValue("@id", MaxId);
                InsertCommentCmd.Parameters.AddWithValue("@project_id", uploaded_files_project_id);
                InsertCommentCmd.Parameters.AddWithValue("@task_id", uploaded_files_task_id);
                InsertCommentCmd.Parameters.AddWithValue("@creator_public", log_in_form.prisijunges_vartotojas);
                InsertCommentCmd.Parameters.AddWithValue("@date", Convert.ToDateTime(DateTime.Today));
                InsertCommentCmd.Parameters.AddWithValue("@comment", log_in_form.prisijunges_vartotojas + " pridėjo failą: " + uploaded_files_file_name);


                if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                InsertCommentCmd.ExecuteNonQuery();
            }
            else
            {
                string InsertCommnetStr = "insert into project_comments (id, project_id, creator_public, date, comment) " +
    "values (@id, @project_id, @creator_public, @date, @comment)";
                NpgsqlCommand InsertCommentCmd = new NpgsqlCommand(InsertCommnetStr, log_in_form.prisijungimas);
                InsertCommentCmd.Parameters.AddWithValue("@id", MaxId);
                InsertCommentCmd.Parameters.AddWithValue("@project_id", uploaded_files_project_id);
                InsertCommentCmd.Parameters.AddWithValue("@creator_public", log_in_form.prisijunges_vartotojas);
                InsertCommentCmd.Parameters.AddWithValue("@date", Convert.ToDateTime(DateTime.Today));
                InsertCommentCmd.Parameters.AddWithValue("@comment", log_in_form.prisijunges_vartotojas + " pridėjo failą: " + uploaded_files_file_name);

                if (log_in_form.prisijungimas.State != System.Data.ConnectionState.Open)
                {
                    log_in_form.prisijungimas.Open();
                }

                InsertCommentCmd.ExecuteNonQuery();
            }
        }



    }
}
