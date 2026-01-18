using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Avalonia.Collections;
using FastMember;
using SukiUI.Helpers;
using TableConverter.Views.Controls.PropertyGrid.ViewModels;
using TableConverter.Views.Controls.PropertyGrid.ViewModels.Attributes;

namespace TableConverter.Views.Controls.PropertyGrid;

public sealed class InstanceViewModel : SukiObservableObject, IDisposable
{
    public INotifyPropertyChanged ViewModel { get; }

    public IAvaloniaReadOnlyList<CategoryViewModel> Categories { get; }
    
    public string[] IgnoreProperties { get; }

    public InstanceViewModel(INotifyPropertyChanged viewModel)
    {
        IgnoreProperties = [];
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Categories = GenerateCategories(viewModel);
    }

    public InstanceViewModel(INotifyPropertyChanged viewModel, string[] ignoreProperties)
    {
        IgnoreProperties = ignoreProperties;
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Categories = GenerateCategories(viewModel);
    }
    
    private static string? GetCategory(Member member)
    {
        var categoryAttr = member.GetAttribute(typeof(CategoryAttribute), false) as CategoryAttribute;
        return categoryAttr?.Category ?? "Properties";
    }

    private static string? GetDisplayName(Member member)
    {
        var displayNameAttr = member.GetAttribute(typeof(DisplayNameAttribute), false) as DisplayNameAttribute;
        return displayNameAttr?.DisplayName;
    }

    public IAvaloniaReadOnlyList<CategoryViewModel> GenerateCategories(INotifyPropertyChanged viewModel)
    {
        var typeAccessor = TypeAccessor.Create(viewModel.GetType());
        var objectAccessor = ObjectAccessor.Create(viewModel);

        var properties = typeAccessor.GetMembers()
            .Where(m => m.CanRead 
                && m.GetAttribute(typeof(IgnoreAttribute), false) is null
                && m.GetAttribute(typeof(NotMappedAttribute), true) is null
                && !IgnoreProperties.Contains(m.Name))
            .ToList();

        var categories = properties
            .Select(member => (Member: member, Category: GetCategory(member), DisplayName: GetDisplayName(member)))
            .Where(p => p.Category is not null)
            .OrderBy(p => p.DisplayName)
            .GroupBy(p => p.Category);

        var categoryViewModels = new AvaloniaList<CategoryViewModel>();

        foreach (var grouping in categories)
        {
            var propertyViewModels = new AvaloniaList<IPropertyViewModel>();

            foreach (var (member, _, displayName) in grouping)
            {
                var propertyViewModel = default(IPropertyViewModel?);
                var propertyName = displayName ?? member.Name;

                if (member.Type == typeof(string))
                {
                    if (member.GetAttribute(typeof(RuntimeValuesAttribute), false) is RuntimeValuesAttribute attribute)
                    {
                        propertyViewModel = new RuntimeValuesViewModel(viewModel, propertyName, attribute.ValuesPath, member, objectAccessor);
                    }
                    else
                    {
                        propertyViewModel = new StringViewModel(viewModel, propertyName, member, objectAccessor);
                    }
                }
                else if (member.Type == typeof(int) || member.Type == typeof(int?))
                {
                    propertyViewModel = new IntegerViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(long) || member.Type == typeof(long?))
                {
                    propertyViewModel = new LongViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(double) || member.Type == typeof(double?))
                {
                    propertyViewModel = new DoubleViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(float) || member.Type == typeof(float?))
                {
                    propertyViewModel = new FloatViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(decimal) || member.Type == typeof(decimal?))
                {
                    propertyViewModel = new DecimalViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(bool) || member.Type == typeof(bool?))
                {
                    propertyViewModel = new BoolViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type.IsEnum)
                {
                    propertyViewModel = new EnumViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(DateTime) || member.Type == typeof(DateTime?))
                {
                    propertyViewModel = new DateTimeViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else if (member.Type == typeof(DateTimeOffset) || member.Type == typeof(DateTimeOffset?))
                {
                    propertyViewModel = new DateTimeOffsetViewModel(viewModel, propertyName, member, objectAccessor);
                }
                else
                {
                    if (objectAccessor[member.Name] is INotifyPropertyChanged propertyValue)
                    {
                        propertyViewModel = new ComplexTypeViewModel(viewModel, propertyName, member, objectAccessor);
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
    
    public void Dispose()
    {
        foreach (var category in Categories)
        {
            foreach (var property in category.Properties)
            {
                property.Dispose();
            }
        }
    }
}
