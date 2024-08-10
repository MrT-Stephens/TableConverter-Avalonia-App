using System.Globalization;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationCountryHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        private readonly string[] CountryNames =
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => new RegionInfo(c.Name))
            .GroupBy(r => r.EnglishName)
            .Select(g => g.First())
            .OrderBy(r => r.EnglishName)
            .Select(r => r.EnglishName)
            .ToArray();

        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => CountryNames[Random.Next(0, CountryNames.Length)], blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
