using System.Xml.Serialization;

namespace Discord_Bot.Services.Models.Wotd;

[XmlRoot(ElementName = "xml")]
public class WotdBase
{
    [XmlElement(ElementName = "words")]
    public Words Words { get; set; }

    [XmlAttribute(AttributeName = "wotd")]
    public string Wotd { get; set; }

    [XmlText]
    public string Text { get; set; }
}
