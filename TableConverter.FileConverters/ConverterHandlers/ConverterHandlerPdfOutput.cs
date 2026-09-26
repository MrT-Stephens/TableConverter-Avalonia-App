using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerPdfOutput : ConverterHandlerOutputAbstract<ConverterHandlerPdfOutputOptions>
{
    public ConverterHandlerPdfOutput()
    {
        Settings.License = LicenseType.Community;
        Settings.CheckIfAllTextGlyphsAreAvailable = false;
        Settings.EnableCaching = true;
        Settings.EnableDebugging = false;
    }

    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

            // QuestPDF composes the document on this thread, so the source is enumerated synchronously
            // as the table is laid out. The enumeration still pages the rows in from the source, so the
            // whole table is never held at once.
            var rows = source.ReadRowsAsync(cancellationToken).ToBlockingEnumerable(cancellationToken);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Content().Table(pdfTable =>
                    {
                        pdfTable.ExtendLastCellsToTableBottom();
                        pdfTable.ColumnsDefinition(columnDefinitions =>
                        {
                            for (var i = 0; i < headers.Count; i++) columnDefinitions.RelativeColumn();
                        });

                        for (uint i = 0; i < headers.Count; i++)
                            if (Options!.BoldHeader)
                                pdfTable.Cell().Row(1).Column(i + 1).Element(Block).Text(headers[(int)i]).ExtraBold()
                                    .FontColor(Color.FromHex(ToHex(Options!.SelectedForegroundColor)));
                            else
                                pdfTable.Cell().Row(1).Column(i + 1).Element(Block).Text(headers[(int)i])
                                    .FontColor(Color.FromHex(ToHex(Options!.SelectedForegroundColor)));

                        var rowIndex = 2u;

                        foreach (var cells in rows)
                        {
                            for (uint j = 0; j < headers.Count; j++)
                            {
                                // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                                var value = j < cells.Length ? cells[j] : string.Empty;

                                pdfTable.Cell().Row(rowIndex).Column(j + 1).Element(Block).Text(value ?? string.Empty)
                                    .FontColor(Color.FromHex(ToHex(Options!.SelectedForegroundColor)));
                            }

                            rowIndex++;
                        }
                    });
                });
            });

            // The caller owns the stream, so generate into it without closing it.
            document.GeneratePdf(stream);
            stream.Flush();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private IContainer Block(IContainer container)
    {
        return container
            .Border(Options!.ShowGridLines ? 1 : 0)
            .Background(Color.FromHex(ToHex(Options!.SelectedBackgroundColor)))
            .ShowOnce()
            .AlignCenter()
            .AlignMiddle();
    }


    private static string ToHex(System.Drawing.KnownColor knownColor)
    {
        var color = System.Drawing.Color.FromKnownColor(knownColor);

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}