using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;

public class EditWeeklyPollOptionModal
{
    public string Title => "Edit Weekly Poll";

    [RequiredInput(true)]
    [InputLabel("Option Title")]
    [ModalTextInput("optiontitle", placeholder: "Title of option shown in poll", maxLength: 55)]
    public string OptionTitle { get; set; }
}
