using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerGeneratedDataInput : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<(List<string>, List<string[]>)>(text);
            });
        }
    }
}
