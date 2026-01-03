using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Microsoft.EntityFrameworkCore;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string Replace = "TableData.Replace";
}

public class ReplaceSearchResultsCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    Utilities.Database.Interfaces.IDbContextFactory<TableStoreDbContext> dbContextFactory) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Replace,
        "Replace",
        "Search table data.",
        "ArrowReturnIcon",
        "View",
        0,
        ["Ctrl+R"],
        canSetLoadingState: true);

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
        
        await using var dbContext = await dbContextFactory.CreateAsync(tableDataViewModel.Path);

        var replacedAmount = await Task.Run(() => ReplaceValuesAsync(replaceValues, settings, tableDataViewModel.Path));

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Replace Success")
            .WithContent($"Replaced {replacedAmount} occurrences with '{settings.ReplaceText}'.")
            .Queue();
        
        tableDataViewModel.InvalidateData();
        searchViewModel.SearchResults.Clear();
    }

    private async Task<int> ReplaceValuesAsync(
        IReadOnlyCollection<TableSearchResult> replaceValues,
        SearchSettingsFrom settings,
        string path)
    {
        if (replaceValues.Count == 0)
        {
            return 0;
        }

        var amount = 0;
        var workers = Math.Min(Environment.ProcessorCount, 8);
        
        var replacements = replaceValues
            .Select(r => new
            {
                r.Row,
                r.Column,
                NewValue = r.Value.Replace(r.FoundValue, settings.ReplaceText)
            })
            .ToList();
        
        var groupedReplacements = replacements
            .GroupBy(r => r.Row)
            .ToList();
        
        await Parallel.ForEachAsync(
            groupedReplacements,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = workers
            },
            async (group, ct) =>
            {
                await using var context = await dbContextFactory.CreateAsync(path, ct);

                foreach (var r in group)
                {
                    if (r.Row <= 0 && settings.ReplaceInHeaders)
                    {
                        await context.Columns
                            .Where(c => c.ColumnId == r.Column)
                            .ExecuteUpdateAsync(s =>
                                s.SetProperty(c => c.Name, r.NewValue), ct);
                    }
                    else if (r.Row > 0 && settings.ReplaceInRows)
                    {
                        await context.Cells
                            .Where(c => c.RowId == r.Row && c.ColumnId == r.Column)
                            .ExecuteUpdateAsync(s =>
                                s.SetProperty(c => c.Value, r.NewValue), ct);
                    }
                }
            });
        
        amount = replacements.Count;

        return amount;
    }
}