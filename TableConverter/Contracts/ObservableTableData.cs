using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Contracts;

public partial class ObservableTableData : ObservableObject, ITableData
{
    #region Properties

    [ObservableProperty] private ObservableCollection<string> _Headers;

    [ObservableProperty] private ObservableCollection<ObservableCollection<object>> _Rows;

    #endregion

    #region Constructors

    public ObservableTableData()
    {
        Headers = [];
        Rows = [];

        Rows.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (var row in e.NewItems.OfType<ObservableCollection<object>>())
                {
                    row.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Rows));
                }
            }

            if (e.OldItems is not null)
            {
                foreach (var row in e.OldItems.OfType<ObservableCollection<object>>())
                {
                    row.CollectionChanged -= (_, __) => OnPropertyChanged(nameof(Rows));
                }
            }

            OnPropertyChanged(nameof(Rows));
        };
    }

    public ObservableTableData(IEnumerable<string> headers, IEnumerable<IEnumerable<object>> rows) 
        : this()
    {
        Headers = headers.ToObservableCollection();
        Rows = [.. rows.Select(row => row.ToObservableCollection())];
    }

    public ObservableTableData(TableData tableData) 
        : this()
    {
        Headers = tableData.GetHeaders().ToObservableCollection();
        Rows = [.. tableData.GetRows().Select(row => row.ToObservableCollection())];
    }

    #endregion

    #region Methods

    public IEnumerable<string> GetHeaders()
    {
        return Headers;
    }

    public IEnumerable<IEnumerable<object>> GetRows()
    {
        return Rows;
    }
    
    public bool IsEmpty()
    {
        return !Rows.Any() || !Headers.Any();
    }
    
    public int GetRowCount()
    {
        return Rows.Count;
    }
    
    public int GetHeaderCount()
    {
        return Headers.Count;
    }

    #endregion
}
