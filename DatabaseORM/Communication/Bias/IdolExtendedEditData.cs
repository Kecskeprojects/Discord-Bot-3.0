namespace DatabaseORM.Communication.Bias;

//An edit of an idol's extra name and debut details, as typed in (the service parses and trims it)
public class IdolExtendedEditData
{
    public string StageName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string KoreanStageName { get; set; } = "";
    public string KoreanFullName { get; set; } = "";
    public string DebutDate { get; set; } = "";
}
