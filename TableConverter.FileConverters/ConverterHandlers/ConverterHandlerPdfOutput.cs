using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerPdfOutput : ConverterHandlerOutputAbstract<ConverterHandlerPdfOutputOptions>
    {
        private Document? PdfDocument { get; set; }

        protected ConverterHandlerPdfOutput()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            QuestPDF.Settings.EnableCaching = true;
            QuestPDF.Settings.EnableDebugging = false;
        }

        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            PdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Content().Table(table =>
                    {
                        table.ExtendLastCellsToTableBottom();
                        table.ColumnsDefinition(columnDefinitions =>
                        {
                            for (var i = 0; i < headers.Length; i++)
                            {
                                columnDefinitions.RelativeColumn();
                            }
                        });

                        for (uint i = 0; i < headers.Length; i++)
                        {
                            if (Options!.BoldHeader)
                            {
                                table.Cell().Row(1).Column(i + 1).Element(Block).Text(headers[i]).ExtraBold().FontColor(Options!.SelectedForegroundColor);
                            }
                            else
                            {
                                table.Cell().Row(1).Column(i + 1).Element(Block).Text(headers[i]).FontColor(Options!.SelectedForegroundColor);
                            }
                        }

                        for (uint i = 0; i < rows.Length; i++)
                        {
                            for (uint j = 0; j < headers.Length; j++)
                            {
                                table.Cell().Row(i + 2).Column(j + 1).Element(Block).Text(rows[i][j]).FontColor(Options!.SelectedForegroundColor);
                            }
                        }
                    });
                });
            });

            return Result<string>.Success($"Please save the '.pdf' file to view the generated file 😁{Environment.NewLine}");
        }

        private IContainer Block(IContainer container)
        {
            return container
                .Border(Options!.ShowGridLines ? 1 : 0)
                .Background(Options!.SelectedBackgroundColor)
                .ShowOnce()
                .AlignCenter()
                .AlignMiddle();
        }

        public override Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            try
            {
                stream.Write(PdfDocument?.GeneratePdf());

                stream.Close();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
            
            return Result.Success();
        }
    }
}
