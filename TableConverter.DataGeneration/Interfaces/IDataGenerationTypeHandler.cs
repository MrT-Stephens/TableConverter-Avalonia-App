namespace TableConverter.DataGeneration.Interfaces
{
    public interface IDataGenerationTypeHandler 
    {
        public dynamic? Options { get; set; }

        public string[] GenerateData(int rows, ushort blanks_percentage);

        public Task<string[]> GenerateDataAsync(int rows, ushort blanks_percentage);
    }
}
