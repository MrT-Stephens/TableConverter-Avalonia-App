using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    public partial class DataGenerationField : ObservableObject
    {
        [ObservableProperty]
        private string _Name = string.Empty;

        [ObservableProperty]
        private string _Type = "Choose a Type";

        [ObservableProperty]
        private int _BlankPercentage = 0;

        [ObservableProperty]
        private IDataGenerationTypeHandler? _TypeHandler = null;
    }
}
