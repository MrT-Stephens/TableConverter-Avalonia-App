using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    internal class ConverterHandlerXmlInputService : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                using (var reader = new StringReader(text))
                using (var data_set = new DataSet())
                {
                    data_set.ReadXml(reader);

                    var table = data_set.Tables[0];

                    foreach (DataColumn column in table.Columns)
                    {
                        headers.Add(column.ColumnName);
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        rows.Add(row.ItemArray.Select(x => x?.ToString() ?? "").ToArray());
                    }
                }

                return (headers, rows);
            });
        }
    }
}
