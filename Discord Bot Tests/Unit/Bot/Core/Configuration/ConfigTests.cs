using Discord_Bot.Core.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Discord_Bot.Tests.Unit.Bot.Core.Configuration;

//The bot's config.json, each test in a folder of its own: a missing value reads as empty, never null, and edits apply without a restart
public sealed class ConfigTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), $"discord-bot-config-{Guid.NewGuid():N}");

    [Fact]
    public void NoFile_WritesAnEmptyOneAndReadsIt()
    {
        Config config = new(folder);

        Assert.True(File.Exists(Path.Combine(folder, "config.json")));
        Assert.Equal(("", "testing", 0), (config.Token, config.Environment, config.VoiceWaitSeconds));
        Assert.Empty(config.Youtube_API_Keys);
        //The generated file has {} here, which the binder reads as null
        Assert.Empty(config.Lastfm_Artist_Input_Replacement);
        Assert.True(config.HeadlessBrowser);
    }

    [Fact]
    public void FileWithValues_GivesThemWhateverTheKeysCase()
    {
        Write("""
            {
              "token": "secret",
              "voice_wait_seconds": 300,
              "Twitch_Client_Id": "twitch-id",
              "youtube_API_Keys": [ "first", "second" ],
              "lastfm_artist_input_replacement": { "bp": "blackpink" },
              "enable_Twitter_Embed": true
            }
            """);

        Config config = new(folder);

        Assert.Equal(("secret", 300, "twitch-id"), (config.Token, config.VoiceWaitSeconds, config.Twitch_Client_Id));
        Assert.Equal(["first", "second"], config.Youtube_API_Keys);
        Assert.Equal("blackpink", config.Lastfm_Artist_Input_Replacement["bp"]);
        Assert.True(config.Enable_Twitter_Embed);
    }

    [Fact]
    public void KeysMissing_ReadAsEmpty()
    {
        Write("{}");

        Config config = new(folder);

        Assert.Equal(("", "", ""), (config.Token, config.Img, config.SqlConnectionString));
        Assert.Empty(config.Youtube_Filter_Words);
        Assert.Empty(config.Lastfm_Artist_Input_Replacement);
        Assert.False(config.HeadlessBrowser);
    }

    [Fact]
    public async Task FileEdited_AppliesWithoutARestart()
    {
        Write("""{ "token": "old" }""");
        Config config = new(folder);

        Write("""{ "token": "new" }""");

        for (int i = 0; config.Token != "new" && i < 100; i++)
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
        Assert.Equal("new", config.Token);
    }

    private void Write([StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        _ = Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "config.json"), json);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(folder, recursive: true);
        }
        catch (IOException)
        {
            //The file watcher may still hold it; the temporary folder is left for the system to clear
        }
    }
}
