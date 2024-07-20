using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;
public class EditIdolExtendedModal : IModal
{
    public string Title => "Edit Idol Extended";

    [RequiredInput(false)]
    [InputLabel("Stage Name")]
    [ModalTextInput("stagename", maxLength: 200)]
    public string StageName { get; set; }

    [RequiredInput(false)]
    [InputLabel("FullName")]
    [ModalTextInput("fullname", maxLength: 100)]
    public string FullName { get; set; }

    [RequiredInput(false)]
    [InputLabel("Korean Stage Name")]
    [ModalTextInput("koreanstagename", maxLength: 100)]
    public string KoreanStageName { get; set; }

    [RequiredInput(false)]
    [InputLabel("Korean Full Name")]
    [ModalTextInput("koreanfullname", maxLength: 100)]
    public string KoreanFullName { get; set; }

    [RequiredInput(false)]
    [InputLabel("Debut Date")]
    [ModalTextInput("debutdate", placeholder: "YYYY. MM. DD. format, latest debut")]
    public string DebutDate { get; set; }
}
