using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationLatitudeHandler : DataGenerationTypeHandlerAbstract
    {
        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() => Random.NextDouble() * 180 - 90, blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
