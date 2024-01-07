using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System;
using TableConverter.Services;
using TableConverter.DataModels;
using System.Linq;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Services

    private readonly ConverterHandlerService ConverterTypesService;
    public TableDataConverterService TableDataConverterService { get; private set; }

    #endregion

    #region Public Properties

    [ObservableProperty]
    private ObservableCollection<string> _ColumnValues;

    [ObservableProperty]
    private ObservableCollection<string[]> _RowValues;

    [ObservableProperty]
    private ObservableCollection<ConverterType> _InputConverterTypes;

    [ObservableProperty]
    private ObservableCollection<ConverterType> _OutputConverterTypes;

    [ObservableProperty]
    private ObservableCollection<string> _InputSearchItems;

    [ObservableProperty]
    private ObservableCollection<string> _OutputSearchItems;

    [ObservableProperty]
    private ConverterType _InputSelectedConverterType;

    [ObservableProperty]
    private ConverterType _OutputSelectedConverterType;

    [ObservableProperty]
    private string _TextBoxConvertedData;

    [ObservableProperty]
    private string _HowToUseText;

    [ObservableProperty]
    private string _HowToUseTitleText;

    [ObservableProperty]
    private string _InfomationTitleText;

    [ObservableProperty]
    private string _InfomationText;

    private string _ConvertedData;
    public string ConvertedData
    {
        get => _ConvertedData;
        set
        {
            SetProperty(ref _ConvertedData, value);
            HandleConverterCompletion(ref _ConvertedData);
        }
    }

    #endregion

    #region Default Constructor

    public MainViewModel(ConverterHandlerService converter_types_service, TableDataConverterService tableConverterService)
    {
        ConverterTypesService = converter_types_service;
        TableDataConverterService = tableConverterService;

        ColumnValues = new ObservableCollection<string>();
        RowValues = new ObservableCollection<string[]>();
        InputConverterTypes = new ObservableCollection<ConverterType>();
        OutputConverterTypes = new ObservableCollection<ConverterType>();
        InputSearchItems = new ObservableCollection<string>();
        OutputSearchItems = new ObservableCollection<string>();

        LoadConverterTypesAsync();

        InputSelectedConverterType = InputConverterTypes[0];
        OutputSelectedConverterType = OutputConverterTypes[1];
    }

    public MainViewModel()
    {
        ConverterTypesService = new ConverterHandlerService();
        TableDataConverterService = new TableDataConverterService();

        ColumnValues = new ObservableCollection<string>();
        RowValues = new ObservableCollection<string[]>();
        InputConverterTypes = new ObservableCollection<ConverterType>();
        OutputConverterTypes = new ObservableCollection<ConverterType>();
        InputSearchItems = new ObservableCollection<string>();
        OutputSearchItems = new ObservableCollection<string>();

        LoadConverterTypesAsync();

        InputSelectedConverterType = InputConverterTypes[0];
        OutputSelectedConverterType = OutputConverterTypes[1];
    }

    #endregion

    #region Misc Functions

    private async void LoadConverterTypesAsync()
    {
        var converter_types = await ConverterTypesService.GetConverterTypesAsync();

        InputConverterTypes = new ObservableCollection<ConverterType>(converter_types.Where((converter_type) => converter_type.convert_from == true));
        OutputConverterTypes = new ObservableCollection<ConverterType>(converter_types.Where((converter_type) => converter_type.convert_to == true));

        InputSearchItems = new ObservableCollection<string>(InputConverterTypes.Select((converter_type) => converter_type.name));
        OutputSearchItems = new ObservableCollection<string>(OutputConverterTypes.Select((converter_type) => converter_type.name));
    }

    private void HandleConverterCompletion(ref string value)
    {
        if (value != null)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            if (bytes.Length > 20000)
            {
                string new_value = Encoding.UTF8.GetString(bytes, 0, 20000);

                new_value += $"{Environment.NewLine}[...]{Environment.NewLine}{Environment.NewLine}Please download or copy to view all generated data 😁{Environment.NewLine}";

                TextBoxConvertedData = new_value;
            }
            else
            {
                TextBoxConvertedData = value;
            }
        }
    }

    partial void OnOutputSelectedConverterTypeChanged(ConverterType value)
    {
        GenerateMenuText();
    }

    partial void OnInputSelectedConverterTypeChanged(ConverterType value)
    {
        GenerateMenuText();
    }

    private void GenerateMenuText()
    {
        InfomationTitleText = $"Convert {InputSelectedConverterType?.name} to {OutputSelectedConverterType?.name}";

        InfomationText = $"This versatile application enables users to easily convert {InputSelectedConverterType?.name} files to {OutputSelectedConverterType?.name} through its intuitive and user-friendly interface. All processing for the convertion is performed locally, ensuring your data remains secure and private without the need for network connectivity.";

        HowToUseTitleText = $"How to convert {InputSelectedConverterType?.name} to {OutputSelectedConverterType?.name}?";

        HowToUseText =
            $"1. Simply open your {InputSelectedConverterType?.name} file—it boasts the file extension '{InputSelectedConverterType?.extension}'. The data seamlessly populates the grid view upon opening, setting the stage for effortless management.{Environment.NewLine}{Environment.NewLine}" +
            $"2. Navigate through your {InputSelectedConverterType?.name} dataset with ease. The 'Edit Data' options offer a user-friendly interface to tailor your data as needed.{Environment.NewLine}{Environment.NewLine}" +
            $"3. When satisfied with your edits, effortlessly transform your {InputSelectedConverterType?.name} data into the desired {OutputSelectedConverterType?.name} file type. Witness the conversion process in real-time, courtesy of the intuitive progress bar.{Environment.NewLine}{Environment.NewLine}" +
            $"4. Upon completion, choose to either save your refined {OutputSelectedConverterType?.name} file with a simple click of the 'Save File' button or swiftly copy the transformed data using the 'Copy' button.{Environment.NewLine}{Environment.NewLine}" +
            $"5. Enjoy your transformed data 😁";
    }

    #endregion
}
