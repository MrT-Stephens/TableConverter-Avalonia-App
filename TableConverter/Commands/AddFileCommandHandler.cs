using System.Threading.Tasks;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.Commands;

public class AddFileCommandHandler(IFilesDialogManager filesDialogManager, IConverterTypes converterTypes) : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata { get; } = new CommandMetadata("AddFile");
    
    public Task Execute(object? parameter, ICommandContext context)
    {
        string? selectedItem = null;
        
        return Task.CompletedTask;
    }

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return true;
    }
}