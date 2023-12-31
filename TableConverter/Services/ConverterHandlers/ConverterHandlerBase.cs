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
        public virtual Task<DataTable> LoadExampleAsync()
        {
            return Task.Run(() =>
            {
                DataTable data_table = new DataTable();

                data_table.Columns.Add("First Name");
                data_table.Columns.Add("Last Name");
                data_table.Columns.Add("Age");
                data_table.Columns.Add("Date of Birth");
                data_table.Columns.Add("Id");

                data_table.Rows.Add("John", "Stephens", 25, new DateTime(1995, 1, 1).ToLongDateString(), 1);
                data_table.Rows.Add("Jane", "Doe", 23, new DateTime(1997, 1, 1).ToLongDateString(), 2);
                data_table.Rows.Add("James", "Smith", 30, new DateTime(1990, 1, 1).ToLongDateString(), 3);
                data_table.Rows.Add("Matt", "Williams", 28, new DateTime(1992, 1, 1).ToLongDateString(), 4);
                data_table.Rows.Add("Mary", "Brown", 27, new DateTime(1993, 1, 1).ToLongDateString(), 5);
                data_table.Rows.Add("Michael", "Jones", 26, new DateTime(1994, 1, 1).ToLongDateString(), 6);
                data_table.Rows.Add("Jessica", "Miller", 24, new DateTime(1996, 1, 1).ToLongDateString(), 7);
                data_table.Rows.Add("David", "Davis", 29, new DateTime(1991, 1, 1).ToLongDateString(), 8);
                data_table.Rows.Add("Ashley", "Wilson", 31, new DateTime(1989, 1, 1).ToLongDateString(), 9);
                data_table.Rows.Add("Chris", "Taylor", 32, new DateTime(1988, 1, 1).ToLongDateString(), 10);
                data_table.Rows.Add("Sarah", "Anderson", 33, new DateTime(1987, 1, 1).ToLongDateString(), 11);
                data_table.Rows.Add("Andrew", "Thomas", 34, new DateTime(1986, 1, 1).ToLongDateString(), 12);
                data_table.Rows.Add("Elizabeth", "Jackson", 35, new DateTime(1985, 1, 1).ToLongDateString(), 13);
                data_table.Rows.Add("Justin", "White", 36, new DateTime(1984, 1, 1).ToLongDateString(), 14);
                data_table.Rows.Add("Samantha", "Harris", 37, new DateTime(1983, 1, 1).ToLongDateString(), 15);

                return data_table;
            });
        }

        public virtual Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.FromResult(string.Empty);
        }

        public virtual Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.FromResult(new DataTable());
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
