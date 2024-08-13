using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationIPAddressHandler : DataGenerationTypeHandlerAbstract<DataGenerationIPAddressOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() =>
                {
                    switch (Options!.SelectedIpType)
                    {
                        default:
                        case "IPv4":
                            {
                                return $"{Random.Next(0, 256)}.{Random.Next(0, 256)}.{Random.Next(0, 256)}.{Random.Next(0, 256)}";
                            }
                        case "IPv6":
                            {
                                string[] groups = new string[8];

                                for (int i = 0; i < 8; i++)
                                {
                                    groups[i] = Random.Next(65536).ToString("X4");
                                }

                                return string.Join(":", groups);
                            }
                    }
                }
                , blanks_percentage);
            }

            return data;
        }
    }
}
