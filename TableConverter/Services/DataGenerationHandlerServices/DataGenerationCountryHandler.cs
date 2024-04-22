using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationCountryHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly string[] CountryNames = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => new RegionInfo(c.Name))
            .GroupBy(r => r.EnglishName)
            .Select(g => g.First())
            .OrderBy(r => r.EnglishName)
            .Select(r => r.EnglishName)
            .ToArray();

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() => CountryNames[Random.Next(0, CountryNames.Length)], blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
