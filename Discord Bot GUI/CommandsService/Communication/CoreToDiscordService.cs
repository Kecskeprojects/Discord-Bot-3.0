using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Instagram;
using Discord_Bot.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.CommandsService.Communication
{
    public class CoreToDiscordService
    {
        public static string CreateBirthdayMessage(BirthdayResource birthday, SocketGuild guild)
        {
            SocketGuildUser user = guild.GetUser(birthday.UserDiscordId);

            Random r = new();
            string baseMessage = StaticLists.BirthdayMessage[r.Next(0, StaticLists.BirthdayMessage.Length)];

            string message = string.Format(baseMessage, user.Mention, (DateTime.UtcNow.Year - birthday.Date.Year).ToString());
            return message;
        }

        public static async Task<InstagramMessageResult> SendInstagramMessageAsync(List<FileAttachment> attachments, string[] files, string url, MessageReference refer, IMessageChannel channel, bool ignoreVideos)
        {
            InstagramMessageResult result = new();
            Node metadata = null;
            string caption = "";
            ReadFiles(attachments, files, ref caption, ref metadata, ignoreVideos);
            string message = GetCaption(url, caption, metadata);

            if (attachments.Count > 0)
            {
                result.HasFileDownloadHappened = true;
                await channel.SendFilesAsync(attachments, message, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                result.ShouldMessageBeSuppressed = true;
            }
            //Ignore videos is a second try at sending so that is when we can know if the post is too large to send
            else if (ignoreVideos == true)
            {
                await channel.SendMessageAsync("Post content too large to send!");
            }

            return result;
        }

        public static string GetCaption(string url, string caption, Node metadata)
        {
            string message = $" **{metadata.Owner.Username}**'s [post](<{url}>)\n\n";
            if (!string.IsNullOrEmpty(caption))
            {
                string text = caption.Split("\n\n#")[0];
                int indexOfTag = text.ToLower().LastIndexOf("tags:");
                if (indexOfTag != -1)
                {
                    text = text[..indexOfTag];
                }

                message += $"{text}\n";
            }
            string currDate = DateTimeOffset.FromUnixTimeSeconds(metadata.TakenAtTimestamp).ToString("yyyy\\.MM\\.dd");
            message = message.Insert(0, currDate);

            if (message.Length > 2000)
            {
                message = message[..1999];
            }

            message = UrlTools.SanitizeText(message);
            return message;
        }

        public static void ReadFiles(List<FileAttachment> attachments, string[] files, ref string caption, ref Node metadata, bool ignoreVideos)
        {
            if (attachments.Count > 0)
            {
                attachments.ForEach(x => x.Dispose());
                attachments.Clear();
            }

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(".txt"))
                {
                    caption = File.ReadAllText(files[i]);
                    continue;
                }
                else if (files[i].EndsWith(".json"))
                {
                    metadata = JsonConvert.DeserializeObject<InstaLoaderBase>(File.ReadAllText(files[i])).Node;
                }
                else if (attachments.Count <= 10 && (!ignoreVideos || files[i].EndsWith(".jpg") || files[i].EndsWith(".png")))
                {
                    attachments.Add(new FileAttachment(files[i]));
                }
            }
        }
    }
}
