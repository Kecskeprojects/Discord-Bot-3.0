namespace DatabaseORM.Enums;

public enum DbProcessResultEnum
{
    Failure = 0,
    Success = 1,
    PartialSuccess = 2,
    NotFound = 3,
    AlreadyExists = 4,
    UpdatedExisting = 5,
    MultipleResults = 6,
    PartialNotFound = 7,
    MultipleExists = 8,
    //Something else still relies on it, so it was left as it is
    InUse = 9
}
