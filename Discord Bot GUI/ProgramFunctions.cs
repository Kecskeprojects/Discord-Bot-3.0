using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Discord_Bot.Logger;

namespace Discord_Bot
{
    public class ProgramFunctions
    {
        private readonly Logging _logging;
        public ProgramFunctions(Logging logging)
        {
            _logging = logging;
        }


        #region Log to file method
        //For logging messages, errors, and messages to log files
        public void LogToFile()
        {
            StreamWriter LogFile_writer = null;
            try
            {
                if (_logging.Logs.Count != 0 && LogFile_writer == null)
                {
                    string file_location = "Logs\\logs" + "[" + DateTime.Now.Year + "-" + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + "-" + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString()) + "].txt";

                    using (LogFile_writer = File.AppendText(file_location)) foreach (string log in _logging.Logs.Select(n => n.Content)) LogFile_writer.WriteLine(log);

                    LogFile_writer = null;
                    _logging.Logs.Clear();
                }
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs LogtoFile", ex.ToString());
            }
        }
        #endregion


        #region File and folder check
        //Check if folders for long term storage exist
        public void Check_Folders()
        {
            List<string> logs = new();
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Logs"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Logs");
                logs.Add("Logs folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets");
                logs.Add("Assets folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Commands"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Commands");
                logs.Add("Commands folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Data"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Data");
                logs.Add("Data folder created!");
            }

            if (logs.Count != 0)
            {
                _logging.Log(string.Join('\n', logs));
            }
        }
        #endregion
    }
}
