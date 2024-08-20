using System.Data;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerXmlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override TableData ReadText(string text)
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

            return new TableData(headers, rows);
        }
    }
}
