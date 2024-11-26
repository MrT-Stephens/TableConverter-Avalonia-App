using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerWordOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
    {
        private XWPFDocument? WordDocument { get; set; }

        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            WordDocument = new XWPFDocument();

            var table = WordDocument.CreateTable(rows.Length + 1, headers.Length);

            for (var i = 0; i < headers.Length; i++)
            {
                table.GetRow(0).GetCell(i).SetText(headers[i]);
            }

            for (var i = 0; i < rows.Length; i++)
            {
                for (var j = 0; j < headers.Length; j++)
                {
                    table.GetRow(i + 1).GetCell(j).SetText(rows[i][j]);
                }
            }

            return Result<string>.Success($"Please save the '.docx' file to view the generated file 😁{Environment.NewLine}");
        }

        public override Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            try
            {
                WordDocument?.Write(stream);

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
