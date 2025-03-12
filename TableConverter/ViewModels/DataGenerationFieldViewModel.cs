using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class DataGenerationFieldViewModel : ObservableObject
{
    #region Methods

    public void SetDataGenerationMethod(DataGenerationMethod? method)
    {
        if (method is null)
        {
            PlaceHolder = "Field Name";
            TypeName = "Choose a Type";
            Name = string.Empty;
            Key = string.Empty;
            Parameters.Clear();
        }
        else
        {
            PlaceHolder = $"Example: {method.Name.Replace(" ", "_")}";
            
            TypeName = method.Name;

            Key = method.Key;

            Parameters = new ObservableCollection<DataGenerationParameterViewModel>(
                method.Parameters.Select(param =>
                    new DataGenerationParameterViewModel(param.Name, param.DefaultValue, param.Type))
            );
        }
    }

    #endregion

    #region Properties

    [ObservableProperty] private string _Key = string.Empty;

    [ObservableProperty] private string _Name = string.Empty;
    
    [ObservableProperty] private string _PlaceHolder = "Field Name";

    [ObservableProperty] private string _TypeName = "Choose a Type";

    [ObservableProperty] private ushort _BlankPercentage;

    [ObservableProperty] private ObservableCollection<DataGenerationParameterViewModel> _Parameters = [];

    #endregion
}