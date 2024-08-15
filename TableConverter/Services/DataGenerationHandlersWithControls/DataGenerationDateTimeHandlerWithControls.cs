using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationDateTimeHandlerWithControls : DataGenerationDateTimeHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {
            var from_uniform_grid = new UniformGrid()
            {
                Rows = 2,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var from_date_picker = new DatePicker()
            {
                VerticalAlignment = VerticalAlignment.Center,
                SelectedDate = Options!.FromDateTime,
            };

            from_date_picker.SelectedDateChanged += (sender, e) =>
            {
                if (sender is DatePicker date_picker && date_picker.SelectedDate.HasValue)
                {
                    Options!.FromDateTime = new DateTime(
                        date_picker.SelectedDate.Value.Year,
                        date_picker.SelectedDate.Value.Month,
                        date_picker.SelectedDate.Value.Day,
                        Options!.FromDateTime.Hour,
                        Options!.FromDateTime.Minute,
                        Options!.FromDateTime.Second
                    );
                }
            };

            var from_time_picker = new TimePicker()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                SelectedTime = Options!.FromDateTime.TimeOfDay,
            };

            from_time_picker.SelectedTimeChanged += (sender, e) =>
            {
                if (sender is TimePicker time_picker && time_picker.SelectedTime.HasValue)
                {
                    Options!.FromDateTime = new DateTime(
                        Options!.FromDateTime.Year,
                        Options!.FromDateTime.Month,
                        Options!.FromDateTime.Day,
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
                Text = " to ",
                VerticalAlignment = VerticalAlignment.Center,
            };

            to_label.Classes.Add("h4");

            var to_uniform_grid = new UniformGrid()
            {
                Rows = 2,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var to_date_picker = new DatePicker()
            {
                VerticalAlignment = VerticalAlignment.Center,
                SelectedDate = Options!.ToDateTime,
            };

            to_date_picker.SelectedDateChanged += (sender, e) =>
            {
                if (sender is DatePicker date_picker && date_picker.SelectedDate.HasValue)
                {
                    Options!.ToDateTime = new DateTime(
                        date_picker.SelectedDate.Value.Year,
                        date_picker.SelectedDate.Value.Month,
                        date_picker.SelectedDate.Value.Day,
                        Options!.ToDateTime.Hour,
                        Options!.ToDateTime.Minute,
                        Options!.ToDateTime.Second
                    );
                }
            };

            var to_time_picker = new TimePicker()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                SelectedTime = Options!.ToDateTime.TimeOfDay,
            };

            to_time_picker.SelectedTimeChanged += (sender, e) =>
            {
                if (sender is TimePicker time_picker && time_picker.SelectedTime.HasValue)
                {
                    Options!.ToDateTime = new DateTime(
                        Options!.ToDateTime.Year,
                        Options!.ToDateTime.Month,
                        Options!.ToDateTime.Day,
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
                ItemsSource = Options!.DateTimeFormats.Keys,
                SelectedItem = Options!.SelectedDateTimeFormat,
                VerticalAlignment = VerticalAlignment.Center,
            };

            format_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_format)
                {
                    Options!.SelectedDateTimeFormat = selected_format;
                }
            };

            OptionsControls.Add(from_uniform_grid);
            OptionsControls.Add(to_label);
            OptionsControls.Add(to_uniform_grid);
            OptionsControls.Add(format_combo_box);
        }
    }
}
