using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;
public class EditGroupModal : IModal
{
    public string Title => "Edit Group";

    [RequiredInput(true)]
    [InputLabel("Name")]
    [ModalTextInput("name", placeholder: "Official name of Group", maxLength: 100)]
    public string Name { get; set; }

    [RequiredInput(false)]
    [InputLabel("Full Name")]
    [ModalTextInput("fullname", maxLength: 100)]
    public string FullName { get; set; }

    [RequiredInput(false)]
    [InputLabel("Full Korean Name")]
    [ModalTextInput("fullkoreanname", maxLength: 100)]
    public string FullKoreanName { get; set; }

    [RequiredInput(false)]
    [InputLabel("Debut Date")]
    [ModalTextInput("debutdate", placeholder: "YYYY. MM. DD. format")]
    public string DebutDate { get; set; }
}
