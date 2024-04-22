using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationDateTimeHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly Dictionary<string, string> DateTimeFormats = new()
        {
            { "Standard", "yyyy-MM-dd HH:mm:ss" },
            { "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss" },
            { "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss" },
            { "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" },
            { "yyyy-MM-dd", "yyyy-MM-dd" },
            { "MM/dd/yyyy", "MM/dd/yyyy" },
            { "dd-MM-yyyy", "dd-MM-yyyy" },
            { "HH:mm:ss", "HH:mm:ss" },
            { "hh:mm:ss tt", "hh:mm:ss tt" },
            { "HH:mm", "HH:mm" },
            { "hh:mm tt", "hh:mm tt" },
            { "MMM dd, yyyy", "MMM dd, yyyy" },
            { "dddd, MMMM dd, yyyy", "dddd, MMMM dd, yyyy" },
            { "yyyy MMMM", "yyyy MMMM" },
            { "MMMM, yyyy", "MMMM, yyyy" },
            { "ISO8601 UTC", "yyyy-MM-ddTHH:mm:ssZ" }
            { "SQL DateTime", "yyyy-MM-dd HH:mm:ss.fff" }
        };

        private DateTime FromDateTime { get; set; } = DateTime.MinValue;

        private DateTime ToDateTime { get; set; } = DateTime.MaxValue;

        private string SelectedDateTimeFormat = "Standard";

        public override void InitializeOptionsControls()
        {
            var from_uniform_grid = new UniformGrid()
            {
                Rows = 2,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var from_date_picker = new DatePicker()
            {
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Stretch,
                SelectedDate = FromDateTime,
            };

            from_date_picker.SelectedDateChanged += (sender, e) =>
            {
                if (sender is DatePicker date_picker && date_picker.SelectedDate.HasValue)
                {
                    FromDateTime = new DateTime(
                        date_picker.SelectedDate.Value.Year, 
                        date_picker.SelectedDate.Value.Month, 
                        date_picker.SelectedDate.Value.Day, 
                        FromDateTime.Hour, 
                        FromDateTime.Minute, 
                        FromDateTime.Second
                    );
                }
            };

            var from_time_picker = new TimePicker()
            {
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedTime = FromDateTime.TimeOfDay,
            };

            from_time_picker.SelectedTimeChanged += (sender, e) =>
            {
                if (sender is TimePicker time_picker && time_picker.SelectedTime.HasValue)
                {
                    FromDateTime = new DateTime(
                        FromDateTime.Year, 
                        FromDateTime.Month, 
                        FromDateTime.Day, 
                        time_picker.SelectedTime.Value.Hours, 
                        time_picker.SelectedTime.Value.Minutes, 
                        time_picker.SelectedTime.Value.Seconds
                    );
                }
            };

            from_uniform_grid.Children.Add(from_time_picker);
            from_uniform_grid.Children.Add(from_date_picker);

            var to_label = new TextBlock()
            {
                Text = "to",
                Margin = new Avalonia.Thickness(20, 0, 20, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var to_uniform_grid = new UniformGrid()
            {
                Rows = 2,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var to_date_picker = new DatePicker()
            {
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Stretch,
                SelectedDate = ToDateTime,
            };

            to_date_picker.SelectedDateChanged += (sender, e) =>
            {
                if (sender is DatePicker date_picker && date_picker.SelectedDate.HasValue)
                {
                    ToDateTime = new DateTime(
                        date_picker.SelectedDate.Value.Year, 
                        date_picker.SelectedDate.Value.Month, 
                        date_picker.SelectedDate.Value.Day, 
                        ToDateTime.Hour, 
                        ToDateTime.Minute, 
                        ToDateTime.Second
                    );
                }
            };

            var to_time_picker = new TimePicker()
            {
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedTime = ToDateTime.TimeOfDay,
            };

            to_time_picker.SelectedTimeChanged += (sender, e) =>
            {
                if (sender is TimePicker time_picker && time_picker.SelectedTime.HasValue)
                {
                    ToDateTime = new DateTime(
                        ToDateTime.Year, 
                        ToDateTime.Month, 
                        ToDateTime.Day, 
                        time_picker.SelectedTime.Value.Hours, 
                        time_picker.SelectedTime.Value.Minutes, 
                        time_picker.SelectedTime.Value.Seconds
                    );
                }
            };

            to_uniform_grid.Children.Add(to_time_picker);
            to_uniform_grid.Children.Add(to_date_picker);

            var format_combo_box = new ComboBox()
            {
                ItemsSource = DateTimeFormats.Keys,
                SelectedItem = SelectedDateTimeFormat,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            format_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_format)
                {
                    SelectedDateTimeFormat = selected_format;
                }
            };

            OptionsControls.Add(from_uniform_grid);
            OptionsControls.Add(to_label);
            OptionsControls.Add(to_uniform_grid);
            OptionsControls.Add(format_combo_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = CheckBlank(() => GenerateRandomDateTime(FromDateTime, ToDateTime).ToString(DateTimeFormats[SelectedDateTimeFormat]), blanks_percentage);
                }

                return data;
            });
        }

        public DateTime GenerateRandomDateTime(DateTime startDate, DateTime endDate)
        {
            // Calculate the range in ticks
            long range = endDate.Ticks - startDate.Ticks;

            // Generate a random offset within the range
            long ticksOffset = (long)(Random.NextDouble() * range);

            // Create a new DateTime by adding the offset to the start date
            return startDate.AddTicks(ticksOffset);
        }

    }
}
