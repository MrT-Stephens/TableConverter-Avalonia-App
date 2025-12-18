using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string Replace = "TableData.Replace";
}

public class ReplaceSearchResultsCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager) : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Replace,
        "Replace",
        "Search table data.",
        "ArrowReturnIcon",
        "View",
        0,
        ["Ctrl+R"]);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItem<TableSearchViewModel>(out var searchViewModel)
               && searchViewModel is { SearchResults.Count: > 0 };
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not IWorkspaceEditor workspace)
        {
            throw new InvalidOperationException("Parent must be a workspace editor.");
        }

        if (!context.TryGetSelectedItem<TableSearchViewModel>(out var searchViewModel))
        {
            workspace.ShowTool<TableSearchViewModel>();
            return;
        }

        if (!context.TryGetSelectedItem<TableDataViewModel>(out var tableDataViewModel))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        if (searchViewModel.SearchResults.Count <= 0)
        {
            context.Cancel("No search results to replace.");
            return;
        }
        
        var settings = searchViewModel.SearchSettings;
        var replaceValues = searchViewModel.SearchResults;

        if (!await dialogManager.CreateDialog()
            .WithTitle("Are you sure?")
            .WithContent($"This will replace {replaceValues.Count} occurrences with '{settings.ReplaceText}'.")
            .WithYesNoResult("Yes", "No")
            .TryShowAsync())
        {
            return;
        }
        
        using var transaction = tableDataViewModel.CreateTransaction();

        var replaceAmount = await Task.Run(() =>
        {
            
            var amount = 0;
            foreach (var value in replaceValues)
            {
                var newValue = value.Value.Replace(value.FoundValue, settings.ReplaceText);

                switch (value.Row)
                {
                    case <= 0 when settings.ReplaceInHeaders:
                        transaction.SetHeader(value.Column, newValue);
                        amount++;
                        break;
                    case > 0 when settings.ReplaceInRows:
                        transaction.SetCell(value.Row - 1, value.Column, newValue);
                        amount++;
                        break;
                }
            }
            
            return amount;
        });
        
        transaction.Commit();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Replace Success")
            .WithContent($"Replaced {replaceAmount} occurrences with '{settings.ReplaceText}'.")
            .Queue();

        searchViewModel.SearchResults.Clear();
    }
}