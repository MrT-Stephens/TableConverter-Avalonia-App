using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationNumberHandler : DataGenerationTypeHandlerAbstract<DataGenerationNumberOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationNumberOptions? options, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (long i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => (Random.NextInt64(options!.MinValue, options!.MaxValue) + Random.NextDouble()).ToString($"N{options!.DecimalPlaces}"), blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
