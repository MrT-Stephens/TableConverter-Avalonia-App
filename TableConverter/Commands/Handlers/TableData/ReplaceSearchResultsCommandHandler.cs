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

        var replacedAmount = await Task.Run(() => ReplaceValuesAsync(dbContext, replaceValues, settings));

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Replace Success")
            .WithContent($"Replaced {replacedAmount} occurrences with '{settings.ReplaceText}'.")
            .Queue();
        
        tableDataViewModel.DataSource.Invalidate();
        searchViewModel.SearchResults.Clear();
    }

    private async Task<int> ReplaceValuesAsync(
        TableStoreDbContext db,
        ICollection<TableSearchResult> replaceValues,
        SearchSettingsFrom settings)
    {
        var amount = 0;
        
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            foreach (var group in replaceValues
                .GroupBy(rv => rv.Row))
            {
                if (settings.ReplaceInHeaders && group.Key <= 0)
                {
                    foreach (var row in group)
                    {
                        var cell = await db.Columns
                            .FirstOrDefaultAsync(tc => tc.ColumnId == row.Column);
                        
                        if (cell is not null)
                        {
                            cell.Name = settings.ReplaceText;
                            amount++;
                        }
                    }
                }
                else if (settings.ReplaceInRows && group.Key > 0)
                {
                    var dbRow = await db.Rows
                        .Include(r => r.Cells)
                        .FirstOrDefaultAsync(tr => tr.RowId == group.Key);

                    if (dbRow is null)
                    {
                        continue;
                    }

                    foreach (var row in group)
                    {
                        var cell = dbRow.Cells
                            .FirstOrDefault(tc => tc.ColumnId == row.Column);

                        if (cell is not null)
                        {
                            cell.Value = settings.ReplaceText;
                            amount++;
                        }
                    }
                }
            }
            
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return amount;
    }
}