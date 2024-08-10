namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationDateTimeOptions : DataGenerationBaseOptions
    {
        public Dictionary<string, string> DateTimeFormats { get; init; } = new()
        {
            { "Standard", "yyyy-MM-dd HH:mm:ss" },
            { "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss" },
            { "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss" },
            { "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" },
            { "yyyy-MM-dd", "yyyy-MM-dd" },
            { "MM/dd/yyyy", "MM/dd/yyyy" },
            { "dd-MM-yyyy", "dd-MM-yyyy" },
            { "HH:mm:ss", "HH:mm:ss" },
            { "hh:mm:ss tt", "hh:mm:ss tt" },
            { "HH:mm", "HH:mm" },
            { "hh:mm tt", "hh:mm tt" },
            { "MMM dd, yyyy", "MMM dd, yyyy" },
            { "dddd, MMMM dd, yyyy", "dddd, MMMM dd, yyyy" },
            { "yyyy MMMM", "yyyy MMMM" },
            { "MMMM, yyyy", "MMMM, yyyy" },
            { "ISO8601 UTC", "yyyy-MM-ddTHH:mm:ssZ" },
            { "SQL DateTime", "yyyy-MM-dd HH:mm:ss.fff" }
        };

        public DateTime FromDateTime { get; set; } = DateTime.MinValue;

        public DateTime ToDateTime { get; set; } = DateTime.MaxValue;

        public string SelectedDateTimeFormat { get; set; } = "Standard";
    }
}
