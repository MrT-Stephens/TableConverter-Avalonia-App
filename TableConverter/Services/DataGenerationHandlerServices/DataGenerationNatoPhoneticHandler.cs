using System;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationNatoPhoneticHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly string[] NatoPhoneticAlphabet = 
        [
            "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliett", "Kilo", "Lima",
            "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey",
            "X-ray", "Yankee", "Zulu"
        ];

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = NatoPhoneticAlphabet[Random.Next(0, NatoPhoneticAlphabet.Length)];
                }

                return data;
            });
        }
    }
}
