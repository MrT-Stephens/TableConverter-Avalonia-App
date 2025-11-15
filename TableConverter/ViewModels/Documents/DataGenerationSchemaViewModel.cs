using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class DataGenerationSchemaViewModel : BaseViewModel, IPaneDocument
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;

    public bool CanClose => false;

    #endregion

    #region Constructors

    public DataGenerationSchemaViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = string.Empty;
        IsEnabled = true;
        IsDirty = false;
    }

    #endregion
}