using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationNatoPhoneticHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        private readonly string[] NatoPhoneticAlphabet = 
        [
            "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliett", "Kilo", "Lima",
            "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey",
            "X-ray", "Yankee", "Zulu"
        ];

        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() => NatoPhoneticAlphabet[Random.Next(0, NatoPhoneticAlphabet.Length)], blanks_percentage);
            }

            return data;
        }
    }
}
