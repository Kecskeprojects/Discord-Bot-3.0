namespace DatabaseORM.Enums;

public enum GenderEnum
{
    Male = 1,
    Female = 2,
    NotSpecified = 3
}

public static class GenderChoiceEnumExtension
{
    public static string ToDatabaseFriendlyString(this GenderEnum genderEnum)
    {
        return genderEnum switch
        {
            GenderEnum.Male => "M",
            GenderEnum.Female => "F",
            GenderEnum.NotSpecified => "Both",
            _ => "Unexpected"
        };
    }

    public static bool EqualsString(this GenderEnum genderEnum, string value)
    {
        return value.Equals(genderEnum.ToDatabaseFriendlyString(), StringComparison.OrdinalIgnoreCase);
    }

    //"M" or "F", in any case; anything else is null
    public static GenderEnum? FromDatabaseString(string value)
    {
        if (GenderEnum.Male.EqualsString(value))
        {
            return GenderEnum.Male;
        }

        if (GenderEnum.Female.EqualsString(value))
        {
            return GenderEnum.Female;
        }

        return null;
    }
}
