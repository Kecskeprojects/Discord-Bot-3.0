using System.Xml.Serialization;

namespace Discord_Bot.Services.Models.Wotd;

[XmlRoot(ElementName = "words")]
public class Words
{
    [XmlElement(ElementName = "date")]
    public string Date { get; set; }

    [XmlElement(ElementName = "langname")]
    public string Langname { get; set; }

    [XmlElement(ElementName = "wordtype")]
    public string Wordtype { get; set; }

    [XmlElement(ElementName = "word")]
    public string Word { get; set; }

    [XmlElement(ElementName = "wordsound")]
    public string Wordsound { get; set; }

    [XmlElement(ElementName = "translation")]
    public string Translation { get; set; }

    [XmlElement(ElementName = "fnphrase")]
    public string Fnphrase { get; set; }

    [XmlElement(ElementName = "phrasesound")]
    public string Phrasesound { get; set; }

    [XmlElement(ElementName = "enphrase")]
    public string Enphrase { get; set; }

    [XmlElement(ElementName = "transliteratedWord")]
    public object TransliteratedWord { get; set; }

    [XmlElement(ElementName = "transliteratedSentence")]
    public object TransliteratedSentence { get; set; }

    [XmlElement(ElementName = "notes")]
    public string Notes { get; set; }
}
