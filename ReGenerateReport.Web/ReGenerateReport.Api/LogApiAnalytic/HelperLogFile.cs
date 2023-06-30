using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.LogApiAnalytic
{
    public static class HelperLogFile
    {
        public static void CreateLog(DateTime currenDate, string refernceCode = null, string logId = null, string functionName = null, string exceptionText = null, System.Exception exception = null, string taskText = null, string policyNo = null)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                //DateTime currenDate = DateTime.Now;
                //string pathLog = Path.Combine(currentPath, "ExceptionLogs");
                string pathLog = Path.Combine(currentPath, "Logs");
                string fullPath = CreateFolder(currenDate, pathLog);
                CreateLogDetail(fullPath, currenDate, refernceCode, logId, functionName, exceptionText, exception, taskText, policyNo);
            }
            catch (Exception) { }
        }

        public static string CreateFolder(DateTime dateCreated, string pathLog)
        {
            try
            {
                string fullPath = string.Empty;
                if (!Directory.Exists(pathLog))
                {
                    Directory.CreateDirectory(pathLog);
                }

                fullPath = CreateFolderByDate(dateCreated, pathLog);

                return fullPath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string CreateFolderByDate(DateTime dateCreated, string pathDirectory)
        {
            try
            {
                string fullPath = string.Empty;

                string dateYear = dateCreated.Year.ToString();
                string dateMonth = dateCreated.Month.ToString().PadLeft(2, '0');
                string dateDay = dateCreated.Day.ToString().PadLeft(2, '0');
                string hour = dateCreated.Hour.ToString().PadLeft(2, '0');
                string minute = dateCreated.Minute.ToString().PadLeft(2, '0');
                string time = $"{hour}_{minute}";

                fullPath = Path.Combine(pathDirectory, dateYear, dateMonth, dateDay, time);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                return fullPath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ExceptionMessage(System.Exception exception)
        {
            try
            {
                string message = string.Empty;

                if (!string.IsNullOrEmpty(exception.Message.ToString()))
                {
                    message = "Message : " + exception.Message.ToString();
                    message += Environment.NewLine;
                }

                if (!string.IsNullOrEmpty(exception.StackTrace.ToString()))
                {
                    message += "StackTrace : " + exception.StackTrace.ToString();
                    message += Environment.NewLine;
                }

                if (exception.InnerException != null)
                {
                    message += "InnerException : " + exception.InnerException.ToString();
                    message += Environment.NewLine;
                }
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public static string TaskMessage(string taskText)
        //{
        //    try
        //    {
        //        string message = string.Empty;

        //        if (!string.IsNullOrEmpty(taskText))
        //        {
        //            message = "Task Event : " + taskText;
        //            message += Environment.NewLine;
        //        }
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public static void CreateLogDetail(string fullPath, DateTime dateCreated, string refernceCode = null, string logId = null, string functionName = null, string exceptionText = null, System.Exception exception = null, string taskText = null, string policyNo = null)
        {
            try
            {
                string textFileName = "Logs.txt";
                string textLogs = string.Empty;

                string path = Path.Combine(fullPath, textFileName);
                if (!File.Exists(path))
                {
                    textLogs = "Create Logs Flow : ";
                    textLogs += "INFO " + dateCreated;
                    textLogs += " (Reference Code , Log Id) : " + logId;
                    System.IO.File.WriteAllText(path, textLogs, Encoding.Unicode);

                }

                if (!string.IsNullOrEmpty(functionName))
                {
                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "==================================================================================================================================================";
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);

                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "INFO " + dateCreated;
                    textLogs += " , (Reference Code , Log Id) : " + refernceCode;
                    if (!string.IsNullOrEmpty(logId))
                        textLogs += ", " + logId;
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);

                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "Method Name : " + functionName;
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);
                }

                if (!string.IsNullOrEmpty(policyNo))
                {
                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "==================================================================================================================================================";
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);
                }

                if (!string.IsNullOrEmpty(taskText))
                {
                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "Task Event " + "[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "] " + ": " + taskText;
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);
                }

                if (!string.IsNullOrEmpty(exceptionText))
                {
                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "ERROR : ";
                    textLogs += Environment.NewLine;
                    textLogs += exceptionText;
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);

                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "==================================================================================================================================================";
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);
                }

                if (exception != null)
                {
                    string strError = ExceptionMessage(exception);
                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "ERROR : ";
                    textLogs += Environment.NewLine;
                    textLogs += strError;
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);

                    textLogs = string.Empty;
                    textLogs += Environment.NewLine;
                    textLogs += "==================================================================================================================================================";
                    System.IO.File.AppendAllText(path, textLogs, Encoding.Unicode);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string ExceptionMessageLog(System.Exception exception)
        {
            try
            {
                string message = string.Empty;

                if (!string.IsNullOrEmpty(exception.Message.ToString()))
                {
                    message = "Message : " + exception.Message.ToString();
                }

                if (!string.IsNullOrEmpty(exception.StackTrace.ToString()))
                {
                    message += "StackTrace : " + exception.StackTrace.ToString();
                    message += " , ";
                }

                if (exception.InnerException != null)
                {
                    message += " , ";
                    message += "InnerException : " + exception.InnerException.ToString();
                }
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
