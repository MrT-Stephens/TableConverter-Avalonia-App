using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationCustomListHandler : DataGenerationTypeHandlerAbstract<DataGenerationCustomListOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationCustomListOptions? options, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() =>
                {
                    string[] items = options!.ItemsList.Split(',');
                    return items[new Random().Next(0, items.Length)];
                }
                , blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
