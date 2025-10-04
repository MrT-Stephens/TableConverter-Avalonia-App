using System.Threading.Tasks;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Interfaces.OverlayService;
using TableConverter.Views.Controls.MessageBox.Enums;

namespace TableConverter.Commands;

public class AddFileCommandHandler(
    IFilesDialogManager filesDialogManager, 
    IConverterTypes converterTypes,
    IOverlayService overlayService) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata { get; } = new CommandMetadata("AddFile");
    
    public async Task Execute(object? parameter, ICommandContext context)
    {
        await overlayService.CreateMessageBox()
            .WithMessage("Adding files...")
            .WithTitle("Please wait")
            .WithButtons(MessageBoxButton.Ok)
            .ShowAsync();
    }

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return true;
    }
}