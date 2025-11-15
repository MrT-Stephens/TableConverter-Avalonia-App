using System.Windows.Input;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Commands.DataModels;

public class CommandInstance : ICommandInstance
{
    #region Properties

    public ICommandMetadata Metadata { get; }
    
    public ICommand Command { get; }
    
    public ICommandHandlerBase Handler { get; }
    
    public ICommandContext Context { get; }

    #endregion

    #region Constructors

    public CommandInstance(ICommand command, ICommandHandlerBase handler, ICommandContext context)
    {
        Command = command;
        Handler = handler;
        Context = context;
        Metadata = handler.CommandMetadata;
    }

    #endregion
}