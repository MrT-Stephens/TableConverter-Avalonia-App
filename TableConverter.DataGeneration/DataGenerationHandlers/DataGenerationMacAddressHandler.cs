using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationMacAddressHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
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
        }
    }
}
