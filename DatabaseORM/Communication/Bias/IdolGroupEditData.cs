namespace DatabaseORM.Communication.Bias;

//The owner's edit of an idol group, as typed in (the service parses and trims it)
public class IdolGroupEditData
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string FullKoreanName { get; set; } = "";
    public string DebutDate { get; set; } = "";
}
