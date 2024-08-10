namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationIPAddressOptions : DataGenerationBaseOptions
    {
        public string[] IpTypes { get; init; } =
        [
            "IPv4",
            "IPv6"
        ];

        public string SelectedIpType { get; set; } = "IPv4";
    }
}
