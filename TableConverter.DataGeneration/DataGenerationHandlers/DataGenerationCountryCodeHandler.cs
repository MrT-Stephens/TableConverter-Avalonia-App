using System.Globalization;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationCountryCodeHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        private readonly string[] CountryCodes =
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => new RegionInfo(c.Name))
            .GroupBy(r => r.EnglishName)
            .Select(g => g.First())
            .OrderBy(r => r.EnglishName)
            .Select(r => r.TwoLetterISORegionName)
            .ToArray();

        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => CountryCodes[Random.Next(0, CountryCodes.Length)], blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
