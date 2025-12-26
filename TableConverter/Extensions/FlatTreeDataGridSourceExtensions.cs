using System;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using TableConverter.Utilities.Database.Models;

namespace TableConverter.Extensions;

public static class FlatTreeDataGridSourceExtensions
{
    public static FlatTreeDataGridSource<TModel> AddAutoColumn<TModel>(
        this FlatTreeDataGridSource<TModel> source,
        object header,
        int columnIndex,
        GridLength? gridLength = null)
        where TModel : class 
    {
        source.Columns.Add(new TemplateColumn<TModel>(
            header, 
            new FuncDataTemplate<RowEntity>((_, _) => new TextBlock
            {
                Background = Brushes.Red,
                Foreground = Brushes.Red,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = $"Item.Cells[{columnIndex}].Value",
                    Mode = BindingMode.TwoWay
                }
            }),
            null,
            GridLength.Star, 
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false
            }));

        return source;
    }
}