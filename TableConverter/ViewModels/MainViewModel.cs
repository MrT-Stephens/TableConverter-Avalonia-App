using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        LoadConverterTypes();

        SelectedInputConverter = InputConverters[0];
        SelectedOutputConverter = OutputConverters[0];
    }

    #region Properties

    [ObservableProperty]
    private ObservableCollection<ConverterType> _InputConverters = new();

    [ObservableProperty]
    private ObservableCollection<ConverterType> _OutputConverters = new();

    [ObservableProperty]
    private ConverterType _SelectedInputConverter;

    [ObservableProperty]
    private ConverterType _SelectedOutputConverter;

    [ObservableProperty]
    private string[] _InputConverterSearchItems = System.Array.Empty<string>();

    [ObservableProperty]
    private string[] _OutputConverterSearchItems = System.Array.Empty<string>();

    [ObservableProperty]
    private string _AboutTitle = string.Empty;

    [ObservableProperty]
    private string _AboutText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StringWithIndex> _AboutHowToConvertText = new();

    [ObservableProperty]
    private string _InputTextBoxWatermarkText = string.Empty;

    [ObservableProperty]
    private string _OutputTextBoxWatermarkText = string.Empty;

    [ObservableProperty]
    private string _InputTextBoxText = string.Empty;

    [ObservableProperty]
    private string _OutputTextBoxText = string.Empty;

    #endregion

    private async void LoadConverterTypes()
    {
        var converters = await ConverterTypesService.GetConverterTypesAsync();

        InputConverters = new ObservableCollection<ConverterType>(converters.Where(c => c.convert_from).ToList());
        OutputConverters = new ObservableCollection<ConverterType>(converters.Where(c => c.convert_to).ToList());

        InputConverterSearchItems = InputConverters.ToList().Select(c => c.name).ToArray();
        OutputConverterSearchItems = OutputConverters.ToList().Select(c => c.name).ToArray();
    }

    partial void OnSelectedInputConverterChanged(ConverterType value)
    {
        GenerateAboutText();
    }

    partial void OnSelectedOutputConverterChanged(ConverterType value)
    {
        GenerateAboutText();
    }

    private void GenerateAboutText()
    {
        AboutTitle = $"Convert {SelectedInputConverter?.name} to {SelectedOutputConverter?.name}";

        AboutText = $"This versatile application enables users to easily convert {SelectedInputConverter?.name} files " +
                    $"to {SelectedOutputConverter?.name} through its intuitive and user-friendly interface. All processing " +
                    "for the conversion is performed locally, ensuring your data remains secure and private without the need " +
                    "for network connectivity.";

        AboutHowToConvertText = [
            new StringWithIndex(0, 
            $"Simply open your {SelectedInputConverter?.name} file—it boasts the file extension " +
            $"'{SelectedInputConverter?.extensions[0]}'. The data seamlessly populates the grid view " +
            "upon opening, setting the stage for effortless management."),

            new StringWithIndex(1,
            $"Navigate through your {SelectedInputConverter?.name} dataset with ease. The 'Edit Data' " +
            "options offer a user-friendly interface to tailor your data as needed."),

            new StringWithIndex(2,
            $"Explore both the 'Edit Data' and 'Output Options' to customize your {SelectedInputConverter?.name} " +
            "dataset. The 'Output Options' allow you to fine-tune settings related to the output file type, " +
            "ensuring your converted data meets your specific requirements."),

            new StringWithIndex(3,
            $"When satisfied with your edits, effortlessly transform your {SelectedInputConverter?.name} " +
            $"data into the desired {SelectedOutputConverter?.name} file type. Witness the conversion process " +
            "in real-time, courtesy of the intuitive progress bar."),

            new StringWithIndex(4,
            $"Upon completion, choose to either save your refined {SelectedOutputConverter?.name} file with a" +
            "simple click of the 'Save File' button or swiftly copy the transformed data using the 'Copy' button."),

            new StringWithIndex(5,
            $"Enjoy your transformed data 😁")
        ];

        InputTextBoxWatermarkText = $"Please open or paste a {SelectedInputConverter?.name} file to begin the conversion process 😊";

        OutputTextBoxWatermarkText = $"Once the {SelectedInputConverter?.name} file is converted you will be able to view the converted {SelectedOutputConverter?.name} file data here 🫠";
    }
}
