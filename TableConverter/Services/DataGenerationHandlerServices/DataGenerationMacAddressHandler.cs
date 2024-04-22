using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationMacAddressHandler : DataGenerationTypeHandlerAbstract
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
                        string[] groups = new string[6];
                        for (int i = 0; i < 6; i++)
                        {
                            groups[i] = Random.Next(0, 256).ToString("X2");
                        }

                        return string.Join(":", groups);
                    }
                    , blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
