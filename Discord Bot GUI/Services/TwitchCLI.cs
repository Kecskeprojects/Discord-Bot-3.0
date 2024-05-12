using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Twitch;
using Discord_Bot.Tools;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Services
{
    public class TwitchCLI(Logging logger) : ITwitchCLI
    {
        private readonly Logging logger = logger;

        //Responsible for generating the access tokens to Twitch's api requests
        public string GenerateToken()
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C twitch.exe token",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")
                }
            };
            process.Start();
            string response = process.StandardError.ReadToEnd();
            process.WaitForExit();

            response = response.Substring(response.IndexOf("Token: ") + 7, 30);
            logger.Query($"Twitch API token: {response}");
            return response;
        }

        //Get user data by username
        public UserData GetChannel(string username)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C twitch.exe api get users?login={username.ToLower()}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")
                }
            };
            process.Start();
            string response = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            User twitchUser = JsonConvert.DeserializeObject<User>(response);

            if (twitchUser == null || CollectionTools.IsNullOrEmpty(twitchUser.Response))
            {
                return null;
            }

            logger.Query($"Twitch user found: {twitchUser.Response[0].DisplayName}");

            return twitchUser.Response[0];
        }
    }
}
