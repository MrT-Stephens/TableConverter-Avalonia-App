using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationHexColourHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
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
        }
    }
}
