using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;
public class EditWeeklyPollModal : IModal
{
    public string Title => "Edit Weekly Poll";

    [RequiredInput(true)]
    [InputLabel("Poll Name")]
    [ModalTextInput("name", placeholder: "Recognizable name for poll", maxLength: 50)]
    public string Name { get; set; }

    [RequiredInput(true)]
    [InputLabel("Poll Title")]
    [ModalTextInput("polltitle", placeholder: "Title shown in poll", maxLength: 300)]
    public string PollTitle { get; set; }

    [RequiredInput(true)]
    [InputLabel("Channel")]
    [ModalTextInput("channel", placeholder: "Where polls will be posted, text channels only", maxLength: 100)]
    public string Channel { get; set; }

    [RequiredInput(false)]
    [InputLabel("Notification Role")]
    [ModalTextInput("role", placeholder: "Role that will be pinged, optional")]
    public string Role { get; set; }
}
