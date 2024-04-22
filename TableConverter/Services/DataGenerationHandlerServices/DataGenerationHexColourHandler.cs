using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationHexColourHandler : DataGenerationTypeHandlerAbstract
    {
        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() =>
                    {
                        return $"#{Random.Next(0, 256):X2}{Random.Next(0, 256):X2}{Random.Next(0, 256):X2}";
                    }
                    , blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
