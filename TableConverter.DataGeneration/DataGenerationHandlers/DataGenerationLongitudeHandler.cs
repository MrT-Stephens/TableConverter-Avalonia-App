using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationLongitudeHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => Random.NextDouble() * 360 - 180, blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
