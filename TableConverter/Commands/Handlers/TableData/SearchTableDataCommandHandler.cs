using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using EFCore.BulkExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string Search = "TableData.Search";
}

public class SearchTableDataCommandHandler(
    ISukiToastManager toastManager,
    Utilities.Database.Interfaces.IDbContextFactory<TableStoreDbContext> dbContextFactory)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Search,
        "Search",
        "Search table data.",
        "SearchIcon",
        "View",
        0,
        ["Ctrl+F"],
        canSetLoadingState: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is IWorkspaceEditor;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not IWorkspaceEditor workspace)
            throw new InvalidOperationException();

        if (!context.TryGetSelectedItem<TableSearchViewModel>(out var searchVm))
        {
            workspace.ShowTool<TableSearchViewModel>();
            return;
        }

        if (!context.TryGetSelectedItem<TableDataViewModel>(out var tableData))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        var settings = searchVm.SearchSettings;
        var searchText = settings.SearchText;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            context.Cancel("Search text is empty.");
            return;
        }

        var matches = await Task.Run(() =>
        {
            return settings switch
            {
                { UseRegularExpressions: true } =>
                    SearchViaRegexAsync(searchText, settings, tableData.Path),
                _ =>
                    SearchAsync(searchText, settings, tableData.Path)
            };
        });

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Search Success")
            .WithContent($"Matched {matches} occurrences with '{settings.SearchText}'.")
            .Queue();
        
        searchVm.DataSource.Invalidate();
    }

    private async Task<int> SearchAsync(string searchText, SearchSettingsFrom settings, string path)
    {
        await using var db = await dbContextFactory.CreateAsync(path);
        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            await db.Database.ExecuteSqlRawAsync("DELETE FROM SEARCH_RESULT;");

            object[] parameters =
            [
                new SqliteParameter("@SEARCH_TEXT", searchText),
                new SqliteParameter("@PATTERN", $"%{searchText}%")
            ];

            var inserted = 0;
            
            if (settings.SearchInHeaders)
            {
                if (settings.MatchWholeWord)
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, COLUMN_ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE NAME = @SEARCH_TEXT;
                            """
                            : """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, COLUMN_ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE NAME COLLATE NOCASE = @SEARCH_TEXT;
                            """,
                        parameters);
                }
                else
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, COLUMN_ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE NAME LIKE @PATTERN;
                            """
                            : """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, COLUMN_ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE NAME COLLATE NOCASE LIKE @PATTERN;
                            """,
                        parameters);
                }
            }
            
            if (settings.SearchInRows)
            {
                if (settings.MatchWholeWord)
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT ROW_ID, COLUMN_ID, VALUE, @SEARCH_TEXT
                            FROM CELLS
                            WHERE VALUE = @SEARCH_TEXT;
                            """
                            : """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT ROW_ID, COLUMN_ID, VALUE, @SEARCH_TEXT
                            FROM CELLS
                            WHERE VALUE COLLATE NOCASE = @SEARCH_TEXT;
                            """,
                        parameters);
                }
                else
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT ROW_ID, COLUMN_ID, VALUE, @SEARCH_TEXT
                            FROM CELLS
                            WHERE VALUE LIKE @PATTERN;
                            """
                            : """
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT ROW_ID, COLUMN_ID, VALUE, @SEARCH_TEXT
                            FROM CELLS
                            WHERE VALUE COLLATE NOCASE LIKE @PATTERN;
                            """,
                        parameters);
                }
            }

            await tx.CommitAsync();
            return inserted;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private async Task<int> SearchViaRegexAsync(string searchText, SearchSettingsFrom settings, string path)
    {
        await using var sharedContext = await dbContextFactory.CreateAsync(path);
        await using var transaction = await sharedContext.Database.BeginTransactionAsync();

        try
        {
            await sharedContext.Database.ExecuteSqlRawAsync("DELETE FROM SEARCH_RESULT;");
            
            var inserted = 0;

            var workers = Math.Min(Environment.ProcessorCount, 8);

            var regex = new Regex(
                settings.MatchWholeWord ? $"^{searchText}$" : searchText,
                settings.MatchCase
                    ? RegexOptions.Compiled
                    : RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (settings.SearchInHeaders)
            {
                var columnResults = new List<SearchResult>();

                var headerQuery = sharedContext.Columns
                    .AsNoTracking()
                    .AsAsyncEnumerable();

                await foreach (var column in headerQuery)
                {
                    if (string.IsNullOrEmpty(column.Name))
                    {
                        continue;
                    }

                    var match = regex.Match(column.Name);

                    if (!match.Success)
                    {
                        continue;
                    }

                    columnResults.Add(new SearchResult
                    {
                        ColumnId = column.ColumnId,
                        RowId = 0,
                        Value = column.Name,
                        FoundValue = match.Value
                    });
                }

                if (columnResults.Count > 0)
                {
                    await sharedContext.BulkInsertAsync(columnResults);
                }

                inserted += columnResults.Count;
            }

            if (settings.SearchInRows)
            {
                var rowResults = new ConcurrentBag<SearchResult>();
                
                var minRow = await sharedContext.Cells.MinAsync(c => c.RowId);
                var maxRow = await sharedContext.Cells.MaxAsync(c => c.RowId);

                var rangeSize = (maxRow - minRow + 1) / workers;

                await Parallel.ForEachAsync(
                    Enumerable.Range(0, workers),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = workers
                    },
                    async (worker, cancellationToken) =>
                    {
                        await using var db = await dbContextFactory.CreateAsync(path, cancellationToken);

                        var start = minRow + worker * rangeSize;

                        var end = worker == workers - 1
                            ? maxRow
                            : start + rangeSize - 1;

                        List<SearchResult> localResults = [];

                        await foreach (var cell in db.Cells
                           .AsNoTracking()
                           .Where(c => c.RowId >= start && c.RowId <= end)
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
                        {
                            if (string.IsNullOrEmpty(cell.Value))
                            {
                                continue;
                            }

                            var match = regex.Match(cell.Value);

                            if (!match.Success)
                            {
                                continue;
                            }

                            localResults.Add(new SearchResult
                            {
                                ColumnId = cell.ColumnId,
                                // RowId starts from 1 in the database, so don't need to add 1 here
                                RowId = cell.RowId,
                                Value = cell.Value!,
                                FoundValue = match.Value
                            });
                        }

                        if (localResults.Count > 0)
                        {
                            foreach (var result in localResults)
                            {
                                rowResults.Add(result);
                            }
                        }
                    });

                if (!rowResults.IsEmpty)
                {
                    await sharedContext.BulkInsertAsync(rowResults);
                    inserted += rowResults.Count;
                }
            }
            
            await transaction.CommitAsync();

            return inserted;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}