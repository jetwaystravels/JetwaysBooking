using DomainLayer.Model;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace OnionConsumeWebAPI.ErrorHandling
{
    public class Logger
    {
        public void WriteLog(Exception ex, string ErrorInformation, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("usp_Exceptionlog", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("ExceptionType", ex.GetType().FullName);
                        cmd.Parameters.AddWithValue("ExceptionMesage", (ex.InnerException == null ? "" : ex.InnerException.Message));
                        cmd.Parameters.AddWithValue("ExceptionSource", (string.IsNullOrEmpty(ex.Source) ? "" : ex.Source ));
                        cmd.Parameters.AddWithValue("ExceptionTargetSiteModule", (ex.TargetSite == null ? "" : ex.TargetSite.Module.Name));
                        cmd.Parameters.AddWithValue("ExceptionTargetsiteName", (ex.TargetSite == null ? "" : ex.TargetSite.Name));
                        cmd.Parameters.AddWithValue("Trace", (ex.StackTrace == null ? "" : ex.StackTrace));
                        cmd.Parameters.AddWithValue("HelpLink", ErrorInformation);
                        cmd.Parameters.AddWithValue("Exceptionlog", ex.ToString());
                        cmd.ExecuteNonQuery();

                    }
                    con.Close();
                }
            }
            catch(Exception exfile)
            {
                FileLog(exfile.ToString());
            }
        }

        public void FileLog(string ErrorMessage)
        {
            var path = Directory.GetCurrentDirectory() + "\\wwwroot\\error.txt";

            if (!Directory.Exists(path))
            {

                System.IO.File.AppendAllText(path, ErrorMessage + Environment.NewLine + "Date :" + DateTime.Now.ToString());
            }
            else
            {
                System.IO.File.AppendAllText(path, ErrorMessage +  Environment.NewLine + "Date :" + DateTime.Now.ToString());
            }
        }

    }
}
