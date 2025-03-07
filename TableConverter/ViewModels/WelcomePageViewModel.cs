using System.Collections.ObjectModel;
using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public class WelcomePageViewModel : BasePageViewModel
{
    #region Constructors

    public WelcomePageViewModel(ConverterTypesService converterTypes, DataGenerationTypesService dataGenerationTypes,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
        ConverterTypes = converterTypes;

        DataGenerationTypes = dataGenerationTypes;
    }

    #endregion

    #region Services

    public ConverterTypesService ConverterTypes { get; }

    public DataGenerationTypesService DataGenerationTypes { get; }

    #endregion

    #region Properties

    public ObservableCollection<string> ConversionSteps { get; } =
    [
        "1. Load Your File – Click 'Open File' or drag and drop a file into TableConverter.",
        "2. Choose an Output Format – Select the desired format from the conversion panel.",
        "3. Preview & Edit (Optional) – Modify data in the tabular editor before exporting.",
        "4. Export the File – Click 'Convert & Save' and choose a destination folder."
    ];

    public ObservableCollection<string> DataGenerationSteps { get; } =
    [
        "1. Go to Data Generator – Click 'Generate Data' from the main menu.",
        "2. Add Fields – Choose from 159+ data types like Names, Emails, Dates, Addresses, etc.",
        "3. Select Locale (Optional) – Generate locale-specific data (en, en_GB, zh_CN).",
        "4. Generate & Export – Click 'Generate' to preview and export as CSV, JSON, Excel, etc."
    ];

    #endregion
}