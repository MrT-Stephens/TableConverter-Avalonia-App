﻿using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerJsonOutputWithControls : ConverterHandlerJsonOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var jsonFormatTypeStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var jsonFormatTypeLabel = new TextBlock
        {
            Text = "JSON File Format:"
        };

        var jsonFormatTypeComboBox = new ComboBox
        {
            ItemsSource = Options!.JsonFormatTypes,
            SelectedItem = Options!.SelectedJsonFormatType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        jsonFormatTypeComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedJsonFormatType)
                Options!.SelectedJsonFormatType = selectedJsonFormatType;
        };

        jsonFormatTypeStackPanel.Children.Add(jsonFormatTypeLabel);
        jsonFormatTypeStackPanel.Children.Add(jsonFormatTypeComboBox);

        Controls?.Add(jsonFormatTypeStackPanel);

        var minifyJsonStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minifyJsonCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.MinifyJson
        };

        minifyJsonCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.MinifyJson = checkBox.IsChecked ?? false;
        };

        var minifyJsonLabel = new TextBlock
        {
            Text = "Minify JSON"
        };

        minifyJsonStackPanel.Children.Add(minifyJsonCheckBox);
        minifyJsonStackPanel.Children.Add(minifyJsonLabel);

        Controls?.Add(minifyJsonStackPanel);
    }
}