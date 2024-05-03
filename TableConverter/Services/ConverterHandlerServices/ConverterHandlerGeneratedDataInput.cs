using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerGeneratedDataInput : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<TableData> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<TableData>(text) ?? new TableData(new(), new());
            });
        }
    }
}
