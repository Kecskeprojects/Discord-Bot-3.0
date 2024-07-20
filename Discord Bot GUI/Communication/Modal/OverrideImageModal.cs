using Discord.Interactions;

namespace Discord_Bot.Communication.Modal;
public class OverrideImageModal : IModal
{
    public string Title => "Override Current Image";

    [RequiredInput(false)]
    [InputLabel("Image URL (Informational)")]
    [ModalTextInput("currentimageurl", placeholder: "current image url, informational input")]
    public string CurrentImageUrl { get; set; }

    [RequiredInput(true)]
    [InputLabel("Image URL")]
    [ModalTextInput("imageurl", placeholder: "image url")]
    public string ImageUrl { get; set; }
}
