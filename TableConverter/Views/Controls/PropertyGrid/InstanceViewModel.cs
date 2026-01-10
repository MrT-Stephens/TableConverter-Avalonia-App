using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using SukiUI.Controls;

namespace TableConverter.Views.Controls.PropertyGrid;

public sealed class InstanceViewModel(INotifyPropertyChanged viewModel) : SukiUI.Controls.InstanceViewModel(viewModel)
{
    private static string? GetCategory(PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes<CategoryAttribute>(false);
        
        var categoryAttributes = attributes as CategoryAttribute[] ?? attributes.ToArray();
        
        if (categoryAttributes.Length != 0)
        {
            return categoryAttributes.First().Category;
        }

        return "Properties";
    }

    private static string? GetDisplayName(PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes<DisplayNameAttribute>(false);
        
        var displayNameAttributes = attributes as DisplayNameAttribute[] ?? attributes.ToArray();
        
        if (displayNameAttributes.Length != 0)
        {
            return displayNameAttributes.First().DisplayName;
        }

        return null;
    }
    
    public override IAvaloniaReadOnlyList<CategoryViewModel> GenerateCategories(INotifyPropertyChanged viewModel)
    {
        var properties = viewModel
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetCustomAttribute<Ignore>() is null)
            .ToList();

        var categories = properties
            .Select(prop => (Property: prop, Category: GetCategory(prop), DisplayName: GetDisplayName(prop)))
            .Where(p => p.Category is not null)
            .Distinct()
            .GroupBy(p => p.Category);

        var categoryViewModels = new AvaloniaList<CategoryViewModel>();
        
        foreach (var grouping in categories)
        {
            var propertyViewModels = new AvaloniaList<IPropertyViewModel>();
            
            foreach (var (property, category, displayName) in grouping)
            {
                var propertyViewModel = default(IPropertyViewModel?);
                var propertyName = displayName ?? property.Name;

                if (property.PropertyType == typeof(string))
                {
                    if (property.GetCustomAttribute<RuntimeValuesAttribute>() is { } attribute)
                    {
                        propertyViewModel = new RuntimeValuesViewModel(viewModel, propertyName, property, attribute.ValuesPath);
                    }
                    else
                    {
                        propertyViewModel = new StringViewModel(viewModel, propertyName, property);
                    }
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    propertyViewModel = new IntegerViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                {
                    propertyViewModel = new LongViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
                {
                    propertyViewModel = new DoubleViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
                {
                    propertyViewModel = new FloatViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    propertyViewModel = new DecimalViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    propertyViewModel = new BoolViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType.IsEnum)
                {
                    propertyViewModel = new EnumViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    propertyViewModel = new DateTimeViewModel(viewModel, propertyName, property);
                }
                else if (property.PropertyType == typeof(DateTimeOffset) ||
                         property.PropertyType == typeof(DateTimeOffset?))
                {
                    propertyViewModel = new DateTimeOffsetViewModel(viewModel, propertyName, property);
                }
                else
                {
                    var propertyValue = property.GetValue(viewModel) as INotifyPropertyChanged;
                    
                    if (propertyValue is { } childViewModel)
                    {
                        propertyViewModel = new ComplexTypeViewModel(viewModel, propertyName, property);
                    }
                }

                if (propertyViewModel is not null)
                {
                    propertyViewModels.Add(propertyViewModel);
                }
            }

            var categoryViewModel = new CategoryViewModel(grouping.Key!, propertyViewModels);
            
            categoryViewModels.Add(categoryViewModel);
        }

        return categoryViewModels;
    }
}