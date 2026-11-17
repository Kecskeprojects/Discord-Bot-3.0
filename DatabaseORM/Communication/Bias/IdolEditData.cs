namespace DatabaseORM.Communication.Bias;

//The owner's edit of an idol's main details, as typed in (the service parses and trims it)
public class IdolEditData
{
    public string ProfileUrl { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public string Gender { get; set; } = "";
}
