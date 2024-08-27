using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using System;

namespace TableConverter.Components.Xaml;

public partial class DataGenerationTypesView : UserControl
{
    public DataGenerationTypesView()
    {
        InitializeComponent();

        AttachedToVisualTree += (sender, args) =>
        {
            if (this.GetVisualRoot() is Window parent)
            {
                parent.GetObservable(TopLevel.ClientSizeProperty).Subscribe(size =>
                {
                    Width = size.Width - 40;
                    Height = size.Height - 75;
                });
            }
        };
    }
}