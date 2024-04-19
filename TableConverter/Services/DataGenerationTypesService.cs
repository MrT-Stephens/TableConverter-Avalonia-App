using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Services.DataGenerationHandlerServices;

namespace TableConverter.Services
{
    internal static class DataGenerationTypesService
    {
        public static Task<Dictionary<DataGenerationType, Type>> GetDataGenerationTypesAsync()
        {
            return Task.FromResult(new Dictionary<DataGenerationType, Type>()
            {
                { 
                    new DataGenerationType("Row Number", "Basic", 
                        "Generates a row number. For example 1, 2, 3."), 
                        typeof(DataGenerationRowNumberHandler)
                },
                {
                    new DataGenerationType("Character Sequence", "Advanced",
                        "Creates a sequence of characters, numbers, and symbols."),
                        typeof(DataGenerationCharacterSequenceHandler)
                }
            });
        }
    }
}
