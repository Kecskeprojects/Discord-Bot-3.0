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
                    string file_location = $"Logs\\logs[{Global.CurrentDate()}].txt";

                    using (LogFile_writer = File.AppendText(file_location))
                    {
                        foreach (string log in _logging.Logs.Select(n => n.Content))
                        {
                            LogFile_writer.WriteLine(log);
                        }
                    }

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
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Logs")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Logs"));
                logs.Add("Logs folder created!");
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets"));
                logs.Add("Assets folder created!");
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets\\Commands")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets\\Commands"));
                logs.Add("Commands folder created!");
            }

            if (logs.Count != 0)
            {
                _logging.Log(string.Join('\n', logs));
            }
        }


        protected List<Uri> LinkSearch(string message, bool ignoreEmbedSuppress, params string[] baseURLs)
        {
            try
            {
                message = message.Replace("www.", "");

                if (baseURLs.Any(x => message.Contains(x)))
                {
                    List<Uri> urls = new();

                    //Going throught the whole message to find all the instagram links
                    int startIndex = 0;
                    while (startIndex != -1)
                    {
                        //We check if there are any links left, one is expected if function gets this far
                        foreach (var baseURL in baseURLs)
                        {
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
