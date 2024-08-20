using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerWordOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
    {
        private XWPFDocument? WordDocument { get; set; } = null;

        public override string Convert(string[] headers, string[][] rows)
        {
            WordDocument = new XWPFDocument();

            var table = WordDocument.CreateTable(rows.Length + 1, headers.Length);

            for (long i = 0; i < headers.LongLength; i++)
            {
                table.GetRow(0).GetCell((int)i).SetText(headers[i]);
            }

            for (long i = 0; i < rows.LongLength; i++)
            {
                for (long j = 0; j < headers.LongLength; j++)
                {
                    table.GetRow((int)i + 1).GetCell((int)j).SetText(rows[i][j]);
                }
            }

            return $"Please save the '.docx' file to view the generated file 😁{Environment.NewLine}";
        }

        public override void SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            WordDocument?.Write(stream);

            stream.Close();
        }
    }
}
