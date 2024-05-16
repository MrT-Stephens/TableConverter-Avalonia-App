using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationMobileOsHandler : DataGenerationTypeHandlerAbstract
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

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() => MobileOperatingSystems[Random.Next(0, MobileOperatingSystems.Length)], blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
