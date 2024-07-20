using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;
public class ChangeIdolGroupModal : IModal
{
    public string Title => "Change  Group";

    [RequiredInput(true)]
    [InputLabel("Group")]
    [ModalTextInput("group", placeholder: "Current group of idol, 'soloist' if none", maxLength: 100)]
    public string Group { get; set; }
}
