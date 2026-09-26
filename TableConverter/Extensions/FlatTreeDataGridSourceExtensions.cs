using System;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
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

    public static FlatTreeDataGridSource<TModel> AddAutoColumn<TModel>(
        this FlatTreeDataGridSource<TModel> source,
        object header,
        string bindingPath,
        bool isReadOnly = false,
        GridLength? gridLength = null,
        UpdateSourceTrigger sourceTrigger = UpdateSourceTrigger.PropertyChanged)
        where TModel : class
    {
        source.Columns.Add(new TemplateColumn<TModel>(
            header,
            new FuncDataTemplate<TModel>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = bindingPath,
                    Mode = BindingMode.OneWay
                },
            }),
            isReadOnly ? null : new FuncDataTemplate<TModel>((_, _) => new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBox.TextProperty] = new Binding
                {
                    Path = bindingPath,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = sourceTrigger
                }
            }),
            gridLength ?? GridLength.Auto,
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false,
            }));
        
        return source;
    }

    /// <summary>
    /// Adds a column whose values are picked from the members of <typeparamref name="TEnum" />, which
    /// the grid shows as their names. Used for a property that names one option rather than holding free
    /// text, so it cannot be filled in with something the application does not understand.
    /// </summary>
    public static FlatTreeDataGridSource<TModel> AddEnumColumn<TModel, TEnum>(
        this FlatTreeDataGridSource<TModel> source,
        object header,
        string bindingPath,
        GridLength? gridLength = null)
        where TModel : class
        where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();

        source.Columns.Add(new TemplateColumn<TModel>(
            header,
            new FuncDataTemplate<TModel>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = bindingPath,
                    Mode = BindingMode.OneWay
                },
            }),
            new FuncDataTemplate<TModel>((_, _) => new ComboBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                ItemsSource = values,
                [!SelectingItemsControl.SelectedItemProperty] = new Binding
                {
                    Path = bindingPath,
                    Mode = BindingMode.TwoWay
                }
            }),
            gridLength ?? GridLength.Auto,
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false,
            }));

        return source;
    }
}