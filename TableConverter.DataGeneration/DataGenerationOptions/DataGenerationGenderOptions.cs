namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationGenderOptions : DataGenerationBaseOptions
    {
        public string[] GenderFormats { get; init; } =
        [
            "Male",
            "M",
            "male",
            "m"
        ];

        public string GenderFormat { get; set; } = "Male";
    }
}
