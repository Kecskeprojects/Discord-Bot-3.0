using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Core
{
    public class ProgramFunctions
    {
        private readonly Logging _logging;
        public ProgramFunctions(Logging logging)
        {
            _logging = logging;
        }


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


        protected List<Uri> LinkSearch(string message, string baseURL, bool ignoreEmbedSuppress)
        {
            try
            {
                message = message.Replace("www.", "");

                if (message.Contains(baseURL))
                {
                    List<Uri> urls = new();

                    //Going throught the whole message to find all the instagram links
                    int startIndex = 0;
                    while (startIndex != -1)
                    {
                        //We check if there are any links left, one is expected
                        startIndex = message.IndexOf(baseURL, startIndex);

                        if (startIndex != -1)
                        {
                            //We cut off anything before the start of the link and replace embed supression characters
                            string beginningCut = message[startIndex..];

                            //And anything after the first space that ended the link
                            string url = beginningCut.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];

                            if (!ignoreEmbedSuppress && !url.Contains('<') && !url.Contains('>'))
                            {
                                url = url.Split('?')[0];
                                urls.Add(new Uri(url));
                            }
                            else if (ignoreEmbedSuppress)
                            {
                                url = url.Replace("<", "").Replace(">", "").Split('?')[0];
                                urls.Add(new Uri(url));
                            }

                            startIndex++;
                        }
                    }

                    return urls;
                }

            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs LinkSearch", ex.ToString());
            }

            return null;
        }
    }
}
