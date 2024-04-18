using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    internal static class DataGenerationTypesService
    {
        public static Task<Dictionary<DataGenerationType, IDataGenerationTypeHandler?>> GetDataGenerationTypesAsync()
        {
            return Task.FromResult(new Dictionary<DataGenerationType, IDataGenerationTypeHandler?>()
            {

            });
        }
    }
}
