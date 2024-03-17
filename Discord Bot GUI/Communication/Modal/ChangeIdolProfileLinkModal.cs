using Discord.Interactions;

namespace Discord_Bot.Communication.Modal
{
    public class ChangeIdolProfileLinkModal : IModal
    {
        public string Title => "Change Profile Link";

        [RequiredInput(true)]
        [InputLabel("Profile URL")]
        [ModalTextInput("profileurl", placeholder: "kpopdb or kprofiles link", maxLength: 200)]
        public string ProfileURL { get; set; }
    }
}
