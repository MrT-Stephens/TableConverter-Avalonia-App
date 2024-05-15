using System;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationTitleHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly string[] Titles =
        [
            "Mr", "Ms", "Mrs", "Miss", "Master", "Madam", "Uncle", "Aunt", "Dr", "Prof", "Doc", "Sir", "Lady", "Lord", "Dame"
        ];

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = CheckBlank(() => Titles[Random.Next(0, Titles.Length - 1)], blanks_percentage);
                }

                return data;
            });
        }
    }
}
