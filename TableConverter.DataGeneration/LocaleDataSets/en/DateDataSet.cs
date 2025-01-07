using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class DateDataSet : DateBase
{
    public override DateEntryDefinition Month { get; } = new(
        [
            "April",
            "August",
            "December",
            "February",
            "January",
            "July",
            "June",
            "March",
            "May",
            "November",
            "October",
            "September"
        ],
        [
            "Apr",
            "Aug",
            "Dec",
            "Feb",
            "Jan",
            "Jul",
            "Jun",
            "Mar",
            "May",
            "Nov",
            "Oct",
            "Sep"
        ],
        [],
        []
    );

    public override DateEntryDefinition Weekday { get; } = new(
        [
            "Friday",
            "Monday",
            "Saturday",
            "Sunday",
            "Thursday",
            "Tuesday",
            "Wednesday"
        ],
        [
            "Fri",
            "Mon",
            "Sat",
            "Sun",
            "Thu",
            "Tue",
            "Wed"
        ],
        [],
        []
    );
}