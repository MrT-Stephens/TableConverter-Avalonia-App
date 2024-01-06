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
                    new string[] { "Thomas", "Anderson", "11/06/1962", "1" },
                    new string[] { "John", "Wick", "09/02/1964", "2" },
                    new string[] { "Neo", "Matrix", "03/01/1973", "3" },
                    new string[] { "John", "McClane", "05/12/1955", "4" },
                    new string[] { "James", "Bond", "11/11/1920", "5" },
                    new string[] { "Jason", "Bourne", "09/04/1970", "6" },
                    new string[] { "Ethan", "Hunt", "07/04/1962", "7" },
                    new string[] { "Indiana", "Jones", "07/01/2002", "8" },
                    new string[] { "Luke", "Skywalker", "09/02/1964", "9" },
                    new string[] { "Han", "Solo", "03/01/1973", "10" },
                    new string[] { "Tony", "Stark", "05/12/1955", "11" },
                    new string[] { "Bruce", "Wayne", "11/11/1920", "12" },
                    new string[] { "Clark", "Kent", "09/04/1970", "13" },
                    new string[] { "Peter", "Parker", "07/04/1962", "14" },
                    new string[] { "Steve", "Rogers", "07/01/2002", "15" }
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
