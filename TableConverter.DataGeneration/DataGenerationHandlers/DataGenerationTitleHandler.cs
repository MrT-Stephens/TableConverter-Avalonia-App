using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationTitleHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        private readonly string[] Titles =
        [
            "Mr", "Ms", "Mrs", "Miss", "Master", "Madam", "Uncle", "Aunt", "Dr", "Prof", "Doc", "Sir", "Lady", "Lord", "Dame"
        ];

        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() => Titles[Random.Next(0, Titles.Length - 1)], blanks_percentage);
            }

            return data;
        }
    }
}
