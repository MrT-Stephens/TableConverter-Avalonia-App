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
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

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

            var searchColumn = settings.SearchInSpecificColumn;
            var searchAllColumns = searchColumn == "All";

            object[] parameters =
            [
                new SqliteParameter("@SEARCH_TEXT", searchText),
                new SqliteParameter("@PATTERN", $"%{searchText}%"),
                new SqliteParameter("@COLUMN", searchColumn)
            ];

            var inserted = 0;
            
            if (settings.SearchInHeaders)
            {
                if (settings.MatchWholeWord)
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE 
                                {(searchAllColumns ? "" : "NAME = @COLUMN AND")} 
                                NAME = @SEARCH_TEXT;
                            """
                            : $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE 
                                {(searchAllColumns ? "" : "NAME = @COLUMN AND")} 
                                NAME COLLATE NOCASE = @SEARCH_TEXT;
                            """,
                        parameters);
                }
                else
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE 
                                {(searchAllColumns ? "" : "NAME = @COLUMN AND")} 
                                NAME LIKE @PATTERN;
                            """
                            : $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT 0, ID, NAME, @SEARCH_TEXT
                            FROM COLUMNS
                            WHERE 
                                {(searchAllColumns ? "" : "NAME = @COLUMN AND")} 
                                NAME COLLATE NOCASE LIKE @PATTERN;
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
                            ? $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT C.ROW_ID, C.COLUMN_ID, C.VALUE, @SEARCH_TEXT
                            FROM CELLS C
                            JOIN COLUMNS COL
                                ON C.COLUMN_ID = COL.ID
                            WHERE 
                                {(searchAllColumns ? "" : "COL.NAME = @COLUMN AND")} 
                                C.VALUE = @SEARCH_TEXT;
                            """
                            : $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT C.ROW_ID, C.COLUMN_ID, C.VALUE, @SEARCH_TEXT
                            FROM CELLS C
                            JOIN COLUMNS COL
                                ON C.COLUMN_ID = COL.ID
                            WHERE 
                                {(searchAllColumns ? "" : "COL.NAME = @COLUMN AND")} 
                                C.VALUE COLLATE NOCASE = @SEARCH_TEXT;
                            """,
                        parameters);
                }
                else
                {
                    inserted += await db.Database.ExecuteSqlRawAsync(
                        settings.MatchCase
                            ? $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT C.ROW_ID, C.COLUMN_ID, C.VALUE, @SEARCH_TEXT
                            FROM CELLS C
                            JOIN COLUMNS COL
                                ON C.COLUMN_ID = COL.ID
                            WHERE 
                                {(searchAllColumns ? "" : "COL.NAME = @COLUMN AND")} 
                                C.VALUE LIKE @PATTERN;
                            """
                            : $"""
                            INSERT INTO SEARCH_RESULT (ROW_ID, COLUMN_ID, VALUE, FOUND_VALUE)
                            SELECT C.ROW_ID, C.COLUMN_ID, C.VALUE, @SEARCH_TEXT
                            FROM CELLS C
                            JOIN COLUMNS COL
                                ON C.COLUMN_ID = COL.ID
                            WHERE 
                                {(searchAllColumns ? "" : "COL.NAME = @COLUMN AND")} 
                                C.VALUE COLLATE NOCASE LIKE @PATTERN;
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

    /// <summary>
    /// Regex search is a lot more inefficient as it needs to load all data
    /// and then perform the search in parallel.
    /// </summary>
    private async Task<int> SearchViaRegexAsync(string searchText, SearchSettingsFrom settings, string path)
    {
        await using var sharedContext = await dbContextFactory.CreateAsync(path);
        await using var transaction = await sharedContext.Database.BeginTransactionAsync();

        try
        {
            await sharedContext.Database.ExecuteSqlRawAsync("DELETE FROM SEARCH_RESULT;");
            
            var inserted = 0;
            var searchColumn = settings.SearchInSpecificColumn;
            var searchAllColumns = searchColumn == "All";

            var workers = Math.Min(Environment.ProcessorCount, 8);

            var regex = new Regex(
                settings.MatchWholeWord ? $"^{searchText}$" : searchText,
                settings.MatchCase
                    ? RegexOptions.Compiled
                    : RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (settings.SearchInHeaders)
            {
                var columnResults = new List<SearchResult>();

                var headersQueryable = sharedContext.Columns.AsNoTracking();

                if (!searchAllColumns)
                {
                    headersQueryable = headersQueryable.Where(c => c.Name == searchColumn);
                }

                await foreach (var column in headersQueryable.AsAsyncEnumerable())
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
                        ColumnId = column.Id,
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

                var minRow = searchAllColumns
                    ? await sharedContext.Cells.MinAsync(c => c.RowId)
                    : await sharedContext.Cells
                        .Include(c => c.Column)
                        .Where(c => c.Column!.Name == searchColumn)
                        .MinAsync(c => c.RowId);
                
                var maxRow = searchAllColumns
                    ? await sharedContext.Cells.MaxAsync(c => c.RowId)
                    : await sharedContext.Cells
                        .Include(c => c.Column)
                        .Where(c => c.Column!.Name == searchColumn)
                        .MaxAsync(c => c.RowId);

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

                        var rowQueryable = db.Cells
                            .AsNoTracking()
                            .Where(c => c.RowId >= start && c.RowId <= end);

                        if (!searchAllColumns)
                        {
                            rowQueryable = rowQueryable
                                .Include(r => r.Column)
                                .Where(r => r.Column!.Name == searchColumn);
                        }

                        await foreach (var cell in rowQueryable
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