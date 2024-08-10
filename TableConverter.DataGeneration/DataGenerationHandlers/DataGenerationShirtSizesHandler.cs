using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationShirtSizesHandler : DataGenerationTypeHandlerAbstract<DataGenerationShirtSizesOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationShirtSizesOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            string[] sizes = (options!.ShirtSizeGroup == "All") ? 
                options!.ShirtSizeGroups.Values.SelectMany(x => x).ToArray() :
                options!.ShirtSizeGroups[options!.ShirtSizeGroup];

            for (int i = 0; i < rows; i++)
            {
                data[i] = (CheckBlank(() => sizes[Random.Next(0, sizes.Length - 1)], blanks_percentage));
            }

            return data;
        }
    }
}
