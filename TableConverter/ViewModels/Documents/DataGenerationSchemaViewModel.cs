using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class DataGenerationSchemaViewModel : BaseDocumentViewModel
{
    #region Properties

    [ObservableProperty] private ObservableCollection<DataGenerationFieldViewModel> _Fields;
    
    public override bool CanClose => false;

    public override bool CanUndo => _undoRedo.CanUndo(Fields);
    public override bool CanRedo => _undoRedo.CanRedo(Fields);

    #endregion

    #region Constructors

    public DataGenerationSchemaViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo)
        : base(commandManager, eventManager, dialogManager, toastManager, undoRedo)
    {
        Fields = [];
    }

    #endregion
}