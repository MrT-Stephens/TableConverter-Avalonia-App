using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.SS.Formula.Functions;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class MarkdownViewModel : BaseDocumentViewModel
{
    #region Properties
    
    [ObservableProperty] private string _Content;

    public override bool CanClose => true;
    public override bool CanUndo => false;
    public override bool CanRedo => false;

    #endregion

    #region Constructors

    public MarkdownViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo) 
        : base(commandManager, eventManager, dialogManager, toastManager, undoRedo)
    {
    }

    #endregion
}