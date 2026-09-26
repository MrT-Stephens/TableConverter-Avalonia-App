using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Commands.DataModels;

public class CommandInstance : ICommandInstance
{
    #region Properties

    public ICommandMetadata Metadata { get; }
    
    public ICommand Command => RelayCommand;
    
    public IRelayCommand RelayCommand { get; }
    
    public ICommandHandlerBase Handler { get; }
    
    public ICommandContext Context { get; }

    #endregion

    #region Constructors

    public CommandInstance(IRelayCommand command, ICommandHandlerBase handler, ICommandContext context)
    {
        RelayCommand = command;
        Handler = handler;
        Context = context;
        Metadata = handler.CommandMetadata;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void RaiseCanExecuteChanged() => RelayCommand.NotifyCanExecuteChanged();

    #endregion
}