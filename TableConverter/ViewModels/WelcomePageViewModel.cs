using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using TableConverter.DataGeneration.DataModels;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    #region Services

    public ConverterTypesService ConverterTypes { get; }

    #endregion

    #region Properties 

    public IReadOnlyDictionary<string, IReadOnlyList<DataGenerationType>> DataGenerationTypes { get; }

    #endregion

    #region Constructors

    public WelcomePageViewModel(ConverterTypesService converterTypes, DataGenerationTypesService dataGenerationTypes, ISukiDialogManager dialogManager, ISukiToastManager toastManager) 
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
        ConverterTypes = converterTypes;

        DataGenerationTypes = dataGenerationTypes.Types
            .Select(val => new KeyValuePair<string, IReadOnlyList<DataGenerationType>>(
                val.Category,
                new List<DataGenerationType>(
                    dataGenerationTypes.Types
                        .Where(types => types.Category == val.Category)
                        .OrderBy(val => val.Name)
                )
            ))
            .DistinctBy(kv => kv.Key)
            .OrderBy(kv => kv.Key)
            .ToDictionary(val => val.Key, val => val.Value);
    }

    #endregion
}
