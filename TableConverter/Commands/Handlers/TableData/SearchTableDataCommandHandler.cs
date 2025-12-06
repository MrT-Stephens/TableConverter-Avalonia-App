using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string Search = "TableData.Search";
}

public class SearchTableDataCommandHandler : ICommandHandler
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Search,
        "Search",
        "Search table data.",
        "SearchIcon",
        "View",
        0,
        ["Ctrl+F"]);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is IWorkspaceEditor;
    }

    public void Execute(object? parameter, ICommandContext context)
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
        
        if (tableDataViewModel.TableData.IsEmpty)
        {
            context.Cancel("No table data document is selected.");
            return;
        }
        
        var settings = searchViewModel.SearchSettings;
        var searchText = settings.SearchText;
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            context.Cancel("Search text is empty.");
            return;
        }

        if (settings is { SearchInHeaders: false, SearchInRows: false })
        {
            context.Cancel("Must at least select rows or columns to search.");
            return;
        }
        
        Regex? regex = null;
        if (settings.UseRegularExpressions)
        {
            var pattern = settings.MatchWholeWord ? $"^{searchText}$" : searchText;
            var options = settings.MatchCase ? RegexOptions.Compiled : (RegexOptions.Compiled | RegexOptions.IgnoreCase);

            try
            {
                regex = new Regex(pattern, options);
            }
            catch
            {
                context.Cancel("Search text is invalid.");
                return;
            }
        }

        var tableColumns = tableDataViewModel.TableData.Columns;
        var tableRows = tableDataViewModel.TableData.Rows;
        
        var results = new ConcurrentBag<TableSearchResult>();

        if (settings.SearchInHeaders)
        {
            for (var colIndex = 0; colIndex < tableColumns.Count; colIndex++)
            {
                var cellText = tableColumns[colIndex].Name;
                
                var match = GetFirstMatch(cellText, searchText, settings, regex);

                if (match is not null)
                {
                    results.Add(new TableSearchResult(colIndex, 0, cellText, match));
                }
            }
        }

        if (settings.SearchInRows)
        {
            Parallel.For(0, tableRows.Count, rowIndex =>
            {
                var row = tableRows[rowIndex];

                for (var colIndex = 0; colIndex < row.Count; colIndex++)
                {
                    var cellText = row[colIndex]?.ToString() ?? "";

                    var match = GetFirstMatch(cellText, searchText, settings, regex);

                    if (match is not null)
                    {
                        results.Add(new TableSearchResult(colIndex, rowIndex + 1, cellText, match));
                    }
                }
            });
        }

        searchViewModel.SearchResults.ClearAndAddRange(results
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Column));
    }
    
    private static string? GetFirstMatch(string cellText, string searchText, SearchSettingsFrom settings, Regex? regex)
    {
        if (settings.UseRegularExpressions && regex != null)
        {
            var m = regex.Match(cellText);
            
            if (m.Success && !string.IsNullOrEmpty(m.Value))
                return m.Value;
            
            return null;
        }

        if (settings.MatchWholeWord)
        {
            return cellText == searchText ? searchText : null;
        }

        var comparison = settings.MatchCase 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;
        
        var index = cellText.IndexOf(searchText, comparison);
        
        return index >= 0 
            ? cellText.Substring(index, searchText.Length) 
            : null;
    }
}