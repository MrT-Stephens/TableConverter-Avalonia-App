using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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

public class SearchTableDataCommandHandler : ICommandHandlerAsync
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
        
        /*if (tableDataViewModel.Rows.Count == 0 || tableDataViewModel.Headers.Count == 0)
        {
            context.Cancel("No table data document is selected.");
            return;
        }*/
        
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

        var results = await Task.Run(() =>
        {
            Regex? regex = null;

            if (settings.UseRegularExpressions)
            {
                var pattern = settings.MatchWholeWord ? $"^{searchText}$" : searchText;
                var options = settings.MatchCase
                    ? RegexOptions.Compiled
                    : RegexOptions.Compiled | RegexOptions.IgnoreCase;

                regex = new Regex(pattern, options);
            }

            var list = new List<TableSearchResult>(1024);

            /*if (settings.SearchInHeaders)
            {
                for (var col = 0; col < headers.Count; col++)
                {
                    var match = GetFirstMatch(headers[col], searchText, settings, regex);
                    
                    if (match != null)
                    {
                        list.Add(new TableSearchResult(col, 0, headers[col], match));
                    }
                }
            }*/

            /*if (settings.SearchInRows)
            {
                for (var row = 0; row < rows.Count; row++)
                {
                    var cells = rows[row];
                    
                    for (var col = 0; col < cells.Length; col++)
                    {
                        var match = GetFirstMatch(cells[col], searchText, settings, regex);
                        
                        if (match != null)
                        {
                            list.Add(new TableSearchResult(col, row + 1, cells[col], match));
                        }
                    }
                }
            }*/

            list.Sort(static (a, b) =>
            {
                var r = a.Row.CompareTo(b.Row);
                return r != 0 ? r : a.Column.CompareTo(b.Column);
            });

            return list;
        });

        searchViewModel.SearchResults = results.ToObservableCollection();
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