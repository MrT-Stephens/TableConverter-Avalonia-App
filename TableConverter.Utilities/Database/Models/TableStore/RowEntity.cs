using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace TableConverter.Utilities.Database.Models.TableStore;

/// <summary>
///     One row of a table store.
/// </summary>
/// <remarks>
///     <para>
///         A row is the unit the grid's data source synchronises back to the store, so an edit to one
///         of its cells is raised as a change of the row. A cell is what an editor binds to, but the
///         row is what gets written, so without that a cell edit would only ever change the copy held
///         in memory.
///     </para>
///     <para>
///         The row watches its cells rather than the other way round: cells are created by whoever
///         fills the store, and only the rows the grid actually shows are worth listening to.
///     </para>
/// </remarks>
public sealed class RowEntity : EntityBaseWithAutoSynchronize<int>
{
    /// <summary>
    ///     The cells currently being watched, so that a cell is never subscribed to twice and a cell
    ///     that leaves the row stops being watched.
    /// </summary>
    private readonly HashSet<CellEntity> _WatchedCells = [];

    private ObservableCollection<CellEntity> _Cells = new();

    public RowEntity()
    {
        _Cells.CollectionChanged += OnCellsCollectionChanged;
        Watch(_Cells);
    }

    public ObservableCollection<CellEntity> Cells
    {
        get => _Cells;
        set
        {
            if (ReferenceEquals(_Cells, value))
            {
                return;
            }

            StopWatching(_Cells);
            _Cells.CollectionChanged -= OnCellsCollectionChanged;

            SetField(ref _Cells, value);

            _Cells.CollectionChanged += OnCellsCollectionChanged;
            Watch(_Cells);
        }
    }

    private void OnCellsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        // A reset names nothing and the collection is already empty by the time it is raised, so
        // everything being watched is dropped and whatever the collection holds now is watched
        // instead.
        if (args.Action is NotifyCollectionChangedAction.Reset)
        {
            StopWatchingAll();
            Watch(_Cells);
            return;
        }

        if (args.OldItems is not null)
        {
            foreach (var cell in args.OldItems.OfType<CellEntity>())
            {
                if (_WatchedCells.Remove(cell))
                {
                    cell.PropertyChanged -= OnCellPropertyChanged;
                }
            }
        }

        if (args.NewItems is not null)
        {
            foreach (var cell in args.NewItems.OfType<CellEntity>())
            {
                if (_WatchedCells.Add(cell))
                {
                    cell.PropertyChanged += OnCellPropertyChanged;
                }
            }
        }
    }

    /// <summary>
    ///     A value that changed in one of the row's cells is a change of the row.
    /// </summary>
    private void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(CellEntity.Value))
        {
            OnPropertyChanged(nameof(Cells));
        }
    }

    private void Watch(IEnumerable<CellEntity> cells)
    {
        foreach (var cell in cells)
        {
            if (_WatchedCells.Add(cell))
            {
                cell.PropertyChanged += OnCellPropertyChanged;
            }
        }
    }

    private void StopWatching(IEnumerable<CellEntity> cells)
    {
        foreach (var cell in cells)
        {
            if (_WatchedCells.Remove(cell))
            {
                cell.PropertyChanged -= OnCellPropertyChanged;
            }
        }
    }

    /// <summary>
    ///     Stops watching every cell, which is what a reset leaves to do because it names none of the
    ///     cells that left.
    /// </summary>
    private void StopWatchingAll()
    {
        foreach (var cell in _WatchedCells)
        {
            cell.PropertyChanged -= OnCellPropertyChanged;
        }

        _WatchedCells.Clear();
    }
}