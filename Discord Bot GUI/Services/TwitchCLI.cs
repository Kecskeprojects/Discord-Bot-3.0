using DatabaseORM.Interface;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Twitch;
using Discord_Bot.Tools.NativeTools;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Services;

public class TwitchCLI(IBotLogger logger) : ITwitchCLI
{
    private readonly IBotLogger logger = logger;

    //Responsible for generating the access tokens to Twitch's api requests
    public string GenerateToken()
    {
        ProcessStartInfo twitchcli = new()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = "cmd.exe",
            Arguments = "/C twitch.exe token",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")
        };

        string response = "";
        using (Process process = Process.Start(twitchcli) ?? throw new InvalidOperationException("Twitch CLI could not be started"))
        {
            response = process.StandardError.ReadToEnd();
            process.WaitForExit();
        }

        response = response.Substring(response.IndexOf("Token: ", StringComparison.Ordinal) + 7, 30);
        logger.Query($"Twitch API token: {response}");
        return response;
    }

    //Get user data by username
    public UserData? GetChannel(string username)
    {
        ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = "cmd.exe",
            Arguments = $"/C twitch.exe api get users?login={username.ToLower()}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")
        };

        string response;
        using (Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Twitch CLI could not be started"))
        {
            response = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        User? twitchUser = JsonConvert.DeserializeObject<User>(response);

        if (twitchUser == null || CollectionTools.IsNullOrEmpty(twitchUser.Response))
        {
            return null;
        }

        logger.Query($"Twitch user found: {twitchUser.Response[0].DisplayName}");

        return twitchUser.Response[0];
    }
}
