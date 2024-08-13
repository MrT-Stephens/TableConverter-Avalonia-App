using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationAppVersionHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() =>
                {
                    string version = Random.Next(0, 50).ToString();

                    if (Random.Next(0, 1) == 1)
                    {
                        version += $".{Random.Next(0, 50)}";
                    }

                    if (Random.Next(0, 1) == 1)
                    {
                        version += $".{Random.Next(0, 50)}";
                    }

                    return version;
                }
                , blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
