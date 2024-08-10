namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationNumberOptions : DataGenerationBaseOptions
    {
        public long MinValue { get; set; } = 1;

        public long MaxValue { get; set; } = 100;

        public short DecimalPlaces { get; set; } = 0;
    }
}
