using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationAppVersionHandler : DataGenerationTypeHandlerAbstract
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
                        string version = Random.Next(0, 50).ToString();

                        if (Random.NextBoolean())
                        {
                            version += $".{Random.Next(0, 50)}";
                        }

                        if (Random.NextBoolean())
                        {
                            version += $".{Random.Next(0, 50)}";
                        }

                        return version;
                    }
                    , blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
