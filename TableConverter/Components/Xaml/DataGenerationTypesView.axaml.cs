using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaEdit.Utils;

namespace TableConverter.Components.Xaml;

public partial class DataGenerationTypesView : UserControl
{
    public DataGenerationTypesView()
    {
        InitializeComponent();

        AttachedToVisualTree += (sender, args) =>
        {
            if (this.GetVisualRoot() is Window parent)
                parent.GetObservable(TopLevel.ClientSizeProperty).Subscribe(size =>
                {
                    Width = size.Width - 80;
                    Height = size.Height - 150;
                });
        };
    }
}