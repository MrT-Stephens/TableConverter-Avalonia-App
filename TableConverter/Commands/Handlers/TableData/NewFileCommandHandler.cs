using System;
using System.Linq;
using System.Threading.Tasks;
using SukiUI.Dialogs;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Extensions;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string NewFile = "TableData.NewFile";
}

public class NewFileCommandHandler(ISukiDialogManager dialogManager) : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.NewFile,
        "New File",
        "Add a new table data file.",
        "AddFile",
        "File",
        0,
        ["Ctrl+N"]);
    
    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is TableWorkspaceEditorViewModel;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not TableWorkspaceEditorViewModel editorViewModel)
            return;

        var settings = new NewFileSettingsTableDataForm();

        var result = await dialogManager.CreateDialog()
            .WithTitle("Add New File")
            .WithForm(settings)
            .Dismiss().ByClickingBackground()
            .WithOkResult("Ok")
            .TryShowAsync();
        
        if (result is false)
            return;

        if (editorViewModel.CreateNewDocumentInstance() is not TableDataViewModel document)
            throw new InvalidOperationException("Document should be of type TableDataViewModel");

        document.Title = settings.Name;
        
        /*if (settings.FillWithNumbers)
        {
            var headers = Enumerable.Range(1, settings.Headers + 1)
                .Select(x => x.ToString())
                .ToList();

            document.Headers = headers.ToObservableCollection();
            document.Rows = Enumerable.Repeat(headers, settings.Rows)
                .Select(x => x.ToArray())
                .ToObservableCollection();
        }
        else
        {
            document.Headers = Enumerable.Repeat(string.Empty, settings.Headers)
                .ToObservableCollection();
            document.Rows = Enumerable.Repeat(Enumerable.Repeat(string.Empty, settings.Headers), settings.Rows)
                .Select(x => x.ToArray())
                .ToObservableCollection();
        }*/
        
        editorViewModel.Documents.Add(document);
        editorViewModel.SelectedDocument = document;
    }
}