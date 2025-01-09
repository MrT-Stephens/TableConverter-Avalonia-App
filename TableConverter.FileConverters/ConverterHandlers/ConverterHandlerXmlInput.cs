using System.Data;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerXmlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        using (var reader = new StringReader(text))
        using (var dataSet = new DataSet())
        {
            dataSet.ReadXml(reader);

            var table = dataSet.Tables[0];

            headers.AddRange(from DataColumn column in table.Columns select column.ColumnName);

            rows.AddRange(
                from DataRow row in table.Rows select row.ItemArray.Select(x => x?.ToString() ?? "").ToArray());
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}