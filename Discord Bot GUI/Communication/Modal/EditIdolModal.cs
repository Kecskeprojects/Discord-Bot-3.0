using Discord.Interactions;
using Discord_Bot.Communication.Bias;

namespace Discord_Bot.Communication.Modal;

public class EditIdolModal : IModal
{
    public string Title => "Edit Idol";

    [RequiredInput(false)]
    [InputLabel("Profile URL")]
    [ModalTextInput("profileurl", placeholder: "kpopdb or kprofiles link", maxLength: 200)]
    public string ProfileURL { get; set; } = "";

    [RequiredInput(true)]
    [InputLabel("Name")]
    [ModalTextInput("name", placeholder: "Official stage name of idol", maxLength: 100)]
    public string Name { get; set; } = "";

    [RequiredInput(true)]
    [InputLabel("Group")]
    [ModalTextInput("group", placeholder: "Current group of idol, 'soloist' if none", maxLength: 100)]
    public string Group { get; set; } = "";

    [RequiredInput(false)]
    [InputLabel("Date Of Birth")]
    [ModalTextInput("dateofbirth", placeholder: "YYYY. MM. DD. format")]
    public string DateOfBirth { get; set; } = "";

    [RequiredInput(false)]
    [InputLabel("Gender")]
    [ModalTextInput("gender", placeholder: "F or M", maxLength: 10)]
    public string Gender { get; set; } = "";

    public IdolEditData ToEditData()
    {
        return new IdolEditData()
        {
            ProfileUrl = ProfileURL,
            Name = Name,
            Group = Group,
            DateOfBirth = DateOfBirth,
            Gender = Gender
        };
    }
}
