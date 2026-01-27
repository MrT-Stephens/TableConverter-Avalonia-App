using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Microsoft.Data.Sqlite;
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
    Utilities.Database.Interfaces.IDatabaseContextFactory<TableStoreDbContext> databaseContextFactory) 
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
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

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
        
        await using var dbContext = await databaseContextFactory.CreateAsync(tableDataViewModel.Path);

        var replacedAmount = await Task.Run(() => ReplaceValuesAsync(dbContext, settings));

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Replace Success")
            .WithContent($"Replaced {replacedAmount} occurrences with '{settings.ReplaceText}'.")
            .Queue();
        
        searchViewModel.DataSource.Invalidate();
        tableDataViewModel.InvalidateData();
    }

    private async Task<int> ReplaceValuesAsync(TableStoreDbContext context, SearchSettingsFrom settings)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var affected = 0;

            if (settings.ReplaceInRows)
            {
                affected += await context.Database.ExecuteSqlRawAsync("""
                    UPDATE CELLS
                    SET VALUE = REPLACE(CELLS.VALUE, SR.FOUND_VALUE, @REPLACE_TEXT)
                    FROM SEARCH_RESULT SR
                    WHERE SR.ROW_ID = CELLS.ROW_ID
                        AND SR.COLUMN_ID = CELLS.COLUMN_ID
                        AND SR.ROW_ID > 0;
                    """,
                    new SqliteParameter("@REPLACE_TEXT", settings.ReplaceText));
            }

            if (settings.ReplaceInHeaders)
            {
                affected += await context.Database.ExecuteSqlRawAsync("""
                    UPDATE COLUMNS
                    SET Name = REPLACE(COLUMNS.NAME, SR.FOUND_VALUE, @REPLACE_TEXT)
                    FROM SEARCH_RESULT SR
                    WHERE SR.ROW_ID = 0
                        AND SR.COLUMN_ID = COLUMNS.ID;
                    """,
                    new SqliteParameter("@REPLACE_TEXT", settings.ReplaceText));
            }
            
            await context.Database.ExecuteSqlRawAsync("DELETE FROM SEARCH_RESULT;");
            await transaction.CommitAsync();
            
            return affected;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}