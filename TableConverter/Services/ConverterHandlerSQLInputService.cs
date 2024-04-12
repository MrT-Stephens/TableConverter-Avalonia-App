using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    public class ConverterHandlerSQLInputService : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                return (headers, rows);
            });
        }
    }
}
