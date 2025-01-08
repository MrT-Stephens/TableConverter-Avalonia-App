using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Tests;

/// <summary>
/// Helper class for test data and methods.
/// </summary>
public static class Utils
{
    public static readonly TableData TestTableData = new(
        ["FIRST_NAME", "LAST_NAME", "GENDER", "COUNTRY_CODE"],
        [
            ["Luxeena", "Binoy", "F", "GB"],
            ["Lisa", "Allen", "F", "GB"],
            ["Richard", "Wood", "M", "GB"],
            ["Luke", "Murphy", "M", "GB"],
            ["Adrian", "Heacock", "M", "GB"],
            ["Elvinas", "Palubinskas", "M", "GB"],
            ["Sian", "Turner", "F", "GB"],
            ["Potar", "Potts", "M", "GB"],
            ["Janis", "Chrisp", "F", "GB"],
            ["Sarah", "Proffitt", "F", "GB"],
            ["Calissa", "Noonan", "F", "GB"],
            ["Andrew", "Connors", "M", "GB"],
            ["Siann", "Tynan", "F", "GB"],
            ["Olivia", "Parry", "F", "GB"]
        ]);
}