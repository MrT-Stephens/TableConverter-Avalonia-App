using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationGenderHandler : DataGenerationTypeHandlerAbstract<DataGenerationGenderOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() =>
                {
                    return Options!.GenderFormat switch
                    {
                        "M" => Random.Next(0, 1) == 1 ? "M" : "F",
                        "m" => Random.Next(0, 1) == 1 ? "m" : "f",
                        "male" => Random.Next(0, 1) == 1 ? "male" : "female",
                        "Male" or _ => Random.Next(0, 1) == 1 ? "Male" : "Female"
                    };
                }, blanks_percentage);
            }

            return data;
        }
    }
}
