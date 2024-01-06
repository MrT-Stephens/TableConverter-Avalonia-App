using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public abstract class ConverterHandlerBase
    {
        public List<Control>? Controls { get; private set; }

        /// <summary>
        /// Initializes the controls for the converter type.
        /// Will be overridden by the converter type.
        /// So that the converter type can add its own controls.
        /// </summary>
        public virtual void InitializeControls()
        {
            Controls = new List<Control>();
        }

        public ConverterHandlerBase()
        {
            InitializeControls();
        }

        /// <summary>
        /// Loads the example data for the converter type.
        /// Can be overridden by the converter type.
        /// </summary>
        /// <returns> Task<DataTable> </returns>
        public virtual Task<(List<string>, List<string[]>)> LoadExampleAsync()
        {
            return Task.Run(() =>
            {
                return (new List<string> { "First Name", "Last Name", "Date of Birth", "Id" }, new List<string[]>
                {
                    new string[] { "John", "Doe", "01/01/2000", "1" },
                    new string[] { "Jane", "Doe", "01/01/2000", "2" },
                    new string[] { "John", "Smith", "01/01/2000", "3" },
                    new string[] { "Jane", "Smith", "01/01/2000", "4" },
                    new string[] { "John", "Doe", "01/01/2000", "5" },
                    new string[] { "Jane", "Doe", "01/01/2000", "6" },
                    new string[] { "John", "Smith", "01/01/2000", "7" },
                    new string[] { "Jane", "Smith", "01/01/2000", "8" },
                    new string[] { "John", "Doe", "01/01/2000", "9" },
                    new string[] { "Jane", "Doe", "01/01/2000", "10" },
                });
            });
        }

        public virtual Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.FromResult(string.Empty);
        }

        public virtual Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.FromResult((new List<string>(), new List<string[]>()));
        }

        public virtual Task SaveFileAsync(IStorageFile output, string data)
        {
            return Task.Run(async () =>
            {
                if (output != null)
                {
                    var file_write = await output.OpenWriteAsync();

                    await file_write.WriteAsync(System.Text.Encoding.UTF8.GetBytes(data));

                    file_write.Close();
                }
            });
        }

        public static double MapValue(double value, double from_low, double from_high, double to_low, double to_high)
        {
            // Ensure the value is within the original range
            value = Math.Max(from_low, Math.Min(from_high, value));

            // Map the value to the new range
            return (value - from_low) / (from_high - from_low) * (to_high - to_low) + to_low;
        }

        public enum TextAlignment
        {
            Left, Center, Right
        }

        public static string AlignText(string text, TextAlignment text_alignment, int amount, char padding_character)
        {
            switch (text_alignment)
            {
                case TextAlignment.Left:
                    text = text.PadRight(amount, padding_character);
                    break;
                case TextAlignment.Center:
                    int leftPadding = (amount - text.Length) / 2;
                    int rightPadding = (amount - text.Length) - leftPadding;
                    text = text.PadLeft(text.Length + leftPadding, padding_character);
                    text = text.PadRight(text.Length + rightPadding, padding_character);
                    break;
                case TextAlignment.Right:
                    text = text.PadLeft(amount, padding_character);
                    break;
            }

            return text;
        }
    }
}
