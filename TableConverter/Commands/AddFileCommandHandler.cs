using System.Threading.Tasks;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.Commands;

public class AddFileCommandHandler(
    IFilesDialogManager filesDialogManager, 
    IConverterTypes converterTypes) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata { get; } = new CommandMetadata("AddFile");
    
    public async Task Execute(object? parameter, ICommandContext context)
    {
    }

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return true;
    }
}