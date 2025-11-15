using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class MarkdownViewModel : BaseViewModel, IPaneDocument
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;
    [ObservableProperty] private string _Content;

    public bool CanClose => true;

    #endregion

    #region Constructors

    public MarkdownViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = string.Empty;
        IsEnabled = true;
        Content = string.Empty;
    }

    #endregion
}