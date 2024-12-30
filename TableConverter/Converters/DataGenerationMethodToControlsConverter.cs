using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using TableConverter.ViewModels;

namespace TableConverter.Converters;

public partial class DataGenerationMethodToControlsConverter : IValueConverter
{
    public static readonly DataGenerationMethodToControlsConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        List<Control> controls = [];

        if (value is not IEnumerable<DataGenerationParameterViewModel> parameters)
            throw new ArgumentException("Value must be an IEnumerable<DataGenerationParameterViewModel>.");

        var data = parameters as DataGenerationParameterViewModel[] ?? parameters.ToArray();

        if (data.Length != 0)
            foreach (var param in data)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 5
                };

                var name = UppercaseLettersRegex().Replace(param.Name, " $1");
                name = string.Concat(name[0].ToString().ToUpper().AsSpan(), name.AsSpan(1)) + ":";

                stackPanel.Children.Add(CreateTextBlock(name, ["h6"]));

                if (param.Type == typeof(string))
                    stackPanel.Children.Add(CreateTextBox(param));
                else if (param.Type == typeof(int) || param.Type == typeof(long) ||
                         param.Type == typeof(float) || param.Type == typeof(double) ||
                         param.Type == typeof(decimal))
                    stackPanel.Children.Add(CreateNumericUpDown(param));
                else if (param.Type == typeof(bool))
                    stackPanel.Children.Add(CreateToggleSwitch(param));
                else if (param.Type.IsEnum) stackPanel.Children.Add(CreateComboBox(param));

                controls.Add(stackPanel);
            }
        else
            controls.Add(CreateTextBlock("No options available.", ["h6"]));

        return controls;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static TextBlock CreateTextBlock(string text, string[] classes)
    {
        var control = new TextBlock
        {
            Text = text
        };

        foreach (var @class in classes)
            control.Classes.Add(@class);

        return control;
    }

    private static TextBox CreateTextBox(DataGenerationParameterViewModel param)
    {
        var control = new TextBox
        {
            Text = string.IsNullOrEmpty(param.Value?.ToString()) ? string.Empty : param.Value.ToString(),
            MinWidth = 150
        };

        control.Bind(TextBox.TextProperty, new Binding
        {
            Path = "Value",
            Mode = BindingMode.TwoWay,
            Source = param
        });

        return control;
    }

    private static NumericUpDown CreateNumericUpDown(DataGenerationParameterViewModel param)
    {
        var control = new NumericUpDown
        {
            Value = decimal.TryParse(param.Value?.ToString(), out var result) ? result : decimal.Zero,
            Increment = 1,
            MinWidth = 150
        };

        control.Bind(TextBox.TextProperty, new Binding
        {
            Path = "Value",
            Mode = BindingMode.TwoWay,
            Source = param
        });

        return control;
    }

    private static ComboBox CreateComboBox(DataGenerationParameterViewModel param)
    {
        var control = new ComboBox
        {
            ItemsSource = Enum.GetValues(param.Type),
            SelectedIndex = 0,
            MinWidth = 150
        };

        control.Bind(SelectingItemsControl.SelectedIndexProperty, new Binding
        {
            Path = "Value",
            Mode = BindingMode.TwoWay,
            Source = param
        });

        return control;
    }

    private static ToggleSwitch CreateToggleSwitch(DataGenerationParameterViewModel param)
    {
        var control = new ToggleSwitch
        {
            IsChecked = bool.TryParse(param.Value?.ToString(), out var result) && result,
            MinWidth = 150
        };

        control.Bind(ToggleButton.IsCheckedProperty, new Binding
        {
            Path = "Value",
            Mode = BindingMode.TwoWay,
            Source = param
        });

        return control;
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex UppercaseLettersRegex();
}