using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerExcelInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override async Task<Result> ReadStreamAsync(
        Stream? stream,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(sink);

        try
        {
            using var workbook = new XSSFWorkbook(stream);

            if (workbook.NumberOfSheets == 0 || workbook.GetSheetAt(0).PhysicalNumberOfRows == 0)
            {
                return Result.Failure("Excel file is empty");
            }

            var sheet = workbook.GetSheetAt(0);

            var headers = new List<string>();
            var begun = false;

            foreach (IRow row in sheet)
            {
                if (row.RowNum == 0)
                {
                    headers.AddRange(row.Cells.Select(cell => cell.ToString() ?? ""));
                    continue;
                }

                // The columns come from the first row, so the sink cannot be started until it has been seen.
                if (!begun)
                {
                    await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                    begun = true;
                }

                var values = new List<string>();

                for (var i = 0; i < headers.Count; i++)
                    values.Add(row.GetCell(i)?.ToString() ?? "");

                await sink.WriteRowAsync(values.ToArray(), cancellationToken).ConfigureAwait(false);
            }

            if (!begun)
            {
                await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
            }

            await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}