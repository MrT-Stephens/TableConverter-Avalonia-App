using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;

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
            new FuncDataTemplate<TModel>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = $"Item.Cells[{columnIndex}].Value",
                    Mode = BindingMode.TwoWay
                },
            }),
            new FuncDataTemplate<TModel>((_, _) => new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBox.TextProperty] = new Binding
                {
                    Path = $"Item.Cells[{columnIndex}].Value",
                    Mode = BindingMode.TwoWay
                }
            }),
            GridLength.Auto, 
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false,
            }));

        return source;
    }
}