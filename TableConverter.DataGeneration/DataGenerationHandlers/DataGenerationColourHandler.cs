using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationColourHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() =>
                {
                    ConsoleColor[] colorsArray = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
                    return colorsArray.GetValue(Random.Next(0, colorsArray.Length))?.ToString();
                }
                , blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
