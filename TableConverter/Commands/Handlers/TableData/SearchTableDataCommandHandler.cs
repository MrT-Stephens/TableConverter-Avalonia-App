using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models;
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

        searchVm.SearchResults = [];
        
        await using var contextDb = await dbContextFactory.CreateAsync(tableData.Path);

        var results = await Task.Run(() =>
        {
            if (settings.UseRegularExpressions)
            {
                return SearchViaRegexAsync(searchText, settings, tableData.Path);
            }
            else
            {
                return SearchAsync(searchText, settings, tableData.Path);
            }
        });
        
        searchVm.SearchResults = results.ToObservableCollection();
    }

    private async Task<IEnumerable<TableSearchResult>> SearchAsync(string searchText, SearchSettingsFrom settings, string path)
    {
        List<TableSearchResult> results = [];
        
        await using var context = await dbContextFactory.CreateAsync(path);

        if (settings.SearchInHeaders)
        {
            var headerQuery = context.Columns.AsNoTracking();
            
            if (settings.MatchWholeWord)
            {
                headerQuery = settings.MatchCase
                    ? headerQuery.Where(c => c.Name == searchText)
                    : headerQuery.Where(c =>
                        EF.Functions.Collate(c.Name, "NOCASE") == searchText);
            }
            else
            {
                var pattern = $"%{searchText}%";

                headerQuery = settings.MatchCase
                    ? headerQuery.Where(c => EF.Functions.Like(c.Name, pattern))
                    : headerQuery.Where(c =>
                        EF.Functions.Like(
                            EF.Functions.Collate(c.Name, "NOCASE"),
                            pattern));
            }

            var headerResults = await headerQuery
                .Select(c => new TableSearchResult(
                    c.ColumnId,
                    0,
                    c.Name,
                    searchText))
                .ToListAsync();

            if (headerResults.Count > 0)
            {
                results.AddRange(headerResults);
            }
        }
        
        if (settings.SearchInRows)
        {
            var query = context.Cells.AsNoTracking();

            if (settings.MatchWholeWord)
            {
                query = settings.MatchCase
                    ? query.Where(c => c.Value == searchText)
                    : query.Where(c =>
                        EF.Functions.Collate(c.Value!, "NOCASE") == searchText);
            }
            else
            {
                var pattern = $"%{searchText}%";

                query = settings.MatchCase
                    ? query.Where(c => EF.Functions.Like(c.Value!, pattern))
                    : query.Where(c =>
                        EF.Functions.Like(
                            EF.Functions.Collate(c.Value!, "NOCASE"),
                            pattern));
            }

            var rowResults = await query
                .Select(c => new TableSearchResult(
                    c.ColumnId,
                    // RowId starts from 1 in the database, so don't need to add 1 here
                    c.RowId,
                    c.Value!,
                    searchText))
                .ToListAsync();

            if (rowResults.Count > 0)
            {  
                results.AddRange(rowResults);
            }
        }

        return results
            .OrderBy(c => c.Row)
            .ThenBy(c => c.Column);
    }

    private async Task<IEnumerable<TableSearchResult>> SearchViaRegexAsync(string searchText, SearchSettingsFrom settings, string path)
    {
        await using var context = await dbContextFactory.CreateAsync(path);
        
        var workers = Math.Min(Environment.ProcessorCount, 8);
            
        var regex = new Regex(
            settings.MatchWholeWord ? $"^{searchText}$" : searchText,
            settings.MatchCase
                ? RegexOptions.Compiled
                : RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
        var results = new ConcurrentBag<TableSearchResult>();

        if (settings.SearchInHeaders)
        {
            var headerQuery = context.Columns
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

                results.Add(new TableSearchResult(
                    column.ColumnId,
                    0,
                    column.Name,
                    match.Value));
            }
        }
        
        if (settings.SearchInRows)
        {
            var minRow = await context.Cells.MinAsync(c => c.RowId);
            var maxRow = await context.Cells.MaxAsync(c => c.RowId);

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

                    List<TableSearchResult> localResults = [];

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

                        localResults.Add(new TableSearchResult(
                            cell.ColumnId,
                            // RowId starts from 1 in the database, so don't need to add 1 here
                            cell.RowId,
                            cell.Value,
                            match.Value));
                    }

                    if (localResults.Count > 0)
                    {
                        foreach (var result in localResults)
                        {
                            results.Add(result);
                        }
                    }
                });
        }

        return results
            .OrderBy(c => c.Row)
            .ThenBy(c => c.Column);
    }
}