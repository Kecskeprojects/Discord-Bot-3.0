using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;

public class EditWeeklyPollOptionPresetModal : IModal
{
    public string Title => "Edit Weekly Poll Option";

    [RequiredInput(true)]
    [InputLabel("Preset Name")]
    [ModalTextInput("name", placeholder: "Recognizable name for preset", maxLength: 100)]
    public string Name { get; set; }

    [RequiredInput(true)]
    [InputLabel("Description")]
    [ModalTextInput("description", placeholder: "Description for preset", maxLength: 100)]
    public string Description { get; set; }
}
