namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationShirtSizesOptions : DataGenerationBaseOptions
    {
        public Dictionary<string, string[]> ShirtSizeGroups { get; init; } = new Dictionary<string, string[]>()
        {
            { "Men's", [ "XS", "S", "M", "L", "XL", "XXL", "XXXL", "XXXXL" ] },
            { "Women's Normal", [ "XS", "S", "M", "L", "XL", "XXL", "XXXL" ] },
            { "Women's Numeric", [ "Size 6", "Size 8", "Size 10", "Size 12", "Size 14", "Size 16", "Size 18" ] },
            { "Children's", [ "Newborn (NB)", "0-3 months", "3-6 months", "6-9 months", "9-12 months", "12-18 months", "18-24 months", "2T", "3T", "4T", "5T" ] },
            { "All", [] }
        };

        public string ShirtSizeGroup { get; set; } = string.Empty;
    }
}
