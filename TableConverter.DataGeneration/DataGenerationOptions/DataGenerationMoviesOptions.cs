namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationMoviesOptions : DataGenerationBaseOptions
    {
        public string[] MovieGenres { get; set; } = Array.Empty<string>();

        public string MovieGenre { get; set; } = string.Empty;
    }
}
