using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerPdfOutput : ConverterHandlerOutputAbstract<ConverterHandlerPdfOutputOptions>
    {
        private Document? PdfDocument { get; set; } = null;

        public ConverterHandlerPdfOutput() : base()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            QuestPDF.Settings.EnableCaching = true;
            QuestPDF.Settings.EnableDebugging = false;
        }

        public override string Convert(string[] headers, string[][] rows)
        {
            PdfDocument = Document.Create(contrainer =>
            {
                contrainer.Page(page =>
                {
                    page.Content().Table(table =>
                    {
                        table.ExtendLastCellsToTableBottom();
                        table.ColumnsDefinition(column_definitions =>
                        {
                            for (long i = 0; i < headers.LongLength; i++)
                            {
                                column_definitions.RelativeColumn();
                            }
                        });

                        for (long i = 0; i < headers.LongLength; i++)
                        {
                            if (Options!.BoldHeader)
                            {
                                table.Cell().Row(1).Column((uint)i + 1).Element(Block).Text(headers[(int)i]).ExtraBold().FontColor(Options!.SelectedForegroundColor);
                            }
                            else
                            {
                                table.Cell().Row(1).Column((uint)i + 1).Element(Block).Text(headers[(int)i]).FontColor(Options!.SelectedForegroundColor);
                            }
                        }

                        for (long i = 0; i < rows.LongLength; i++)
                        {
                            for (long j = 0; j < headers.LongLength; j++)
                            {
                                table.Cell().Row((uint)i + 2).Column((uint)j + 1).Element(Block).Text(rows[i][j]).FontColor(Options!.SelectedForegroundColor);
                            }
                        }
                    });
                });
            });

            return $"Please save the '.pdf' file to view the generated file 😁{Environment.NewLine}";
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

        public override void SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            stream.Write(PdfDocument?.GeneratePdf());
                        
            stream.Close();
        }
    }
}
