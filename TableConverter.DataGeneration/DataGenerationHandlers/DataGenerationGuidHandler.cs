using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationGuidHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => Guid.NewGuid().ToString(), blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
