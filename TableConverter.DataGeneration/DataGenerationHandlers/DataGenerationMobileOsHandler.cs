using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationMobileOsHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        private readonly string[] MobileOperatingSystems =
        [
            "Android",
            "iOS",
            "Windows Phone",
            "BlackBerry OS",
            "BlackBerry 10",
            "Symbian OS",
            "Ubuntu Touch",
            "Firefox OS",
            "KaiOS"
        ];

        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            List<string> data = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                data.Add(CheckBlank(() => MobileOperatingSystems[Random.Next(0, MobileOperatingSystems.Length)], blanks_percentage));
            }

            return data.ToArray();
        }
    }
}
