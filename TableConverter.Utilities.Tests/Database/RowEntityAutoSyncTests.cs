using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ModelFlow.DataVirtualization;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers the contract that makes a grid cell edit reach the store: an editor writes to a cell, but
/// the row is the unit that is synchronised, so the row has to report that change as its own. Without
/// that a cell edit would only ever change the copy held in memory and be lost on reload.
/// </summary>
public class RowEntityAutoSyncTests
{
    private static CellEntity NewCell(int columnId, string? value)
    {
        return new CellEntity
        {
            Id = columnId,
            RowId = 1,
            ColumnId = columnId,
            Value = value
        };
    }

    /// <summary>
    /// A data source that stands in for the store and records what auto synchronisation writes back.
    /// </summary>
    private sealed class RecordingRowDataSource(IReadOnlyList<RowEntity> rows)
        : DataSource<RowEntity>(rows.Count, 2, autoSync: true)
    {
        public List<(int RowId, string? Value)> Updates { get; } = [];

        protected override Task<bool> ContainsAsync(RowEntity item)
        {
            return Task.FromResult(rows.Any(row => row.Id == item.Id));
        }

        protected override Task<int> GetCountAsync(Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterQuery)
        {
            return Task.FromResult(rows.Count);
        }

        protected override Task<IEnumerable<RowEntity>> GetItemsAtAsync(
            int offset,
            int count,
            Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterSortQuery)
        {
            return Task.FromResult(rows.Skip(offset).Take(count));
        }

        public override Task<RowEntity?> GetItemAsync(Expression<Func<RowEntity, bool>> predicate)
        {
            return Task.FromResult(rows.AsQueryable().FirstOrDefault(predicate));
        }

        protected override RowEntity GetPlaceHolder(int index, int page, int offset)
        {
            return rows[index];
        }

        protected override bool ModelsEqual(RowEntity a, RowEntity b)
        {
            return a.Id == b.Id;
        }

        protected override Task<bool> DoCreateAsync(RowEntity item)
        {
            return Task.FromResult(true);
        }

        protected override Task<bool> DoUpdateAsync(RowEntity viewModel)
        {
            Updates.Add((viewModel.Id, viewModel.Cells.Count > 0 ? viewModel.Cells[0].Value : null));
            return Task.FromResult(true);
        }

        protected override Task<bool> DoDeleteAsync(RowEntity item)
        {
            return Task.FromResult(true);
        }
    }

    [Fact]
    public void A_Row_Can_Be_Auto_Synchronised()
    {
        var row = new RowEntity();

        // The data source only wraps a materialised item for auto synchronisation when it implements
        // IAutoSynchronize, and it refuses to wrap one that claims to be managed already.
        Assert.IsAssignableFrom<IAutoSynchronize>(row);
        Assert.True(row.CanSave);
        Assert.False(row.IsManaged);
    }

    [Fact]
    public void Editing_A_Cell_Is_Raised_As_A_Change_Of_The_Row()
    {
        var row = new RowEntity();
        var changed = new List<string?>();
        row.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        row.Cells.Add(NewCell(1, "before"));
        row.Cells[0].Value = "after";

        Assert.Contains(nameof(RowEntity.Cells), changed);
    }

    [Fact]
    public void A_Cell_That_Left_The_Row_Stops_Raising_The_Row()
    {
        var row = new RowEntity();
        var cell = NewCell(1, "before");
        row.Cells.Add(cell);
        row.Cells.Clear();

        var changed = new List<string?>();
        row.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        cell.Value = "after";

        Assert.DoesNotContain(nameof(RowEntity.Cells), changed);
    }

    [Fact]
    public void Replacing_The_Cells_Watches_The_New_Cells_And_Not_The_Old_Ones()
    {
        var row = new RowEntity();
        var original = NewCell(1, "one");
        row.Cells.Add(original);

        row.Cells = new ObservableCollection<CellEntity> { NewCell(1, "two") };

        var changed = new List<string?>();
        row.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        original.Value = "one edited";

        Assert.DoesNotContain(nameof(RowEntity.Cells), changed);

        row.Cells[0].Value = "two edited";

        Assert.Contains(nameof(RowEntity.Cells), changed);
    }

    [Fact]
    public async Task Editing_A_Cell_Is_Written_Back_Through_Auto_Synchronisation()
    {
        var previousExecute = VirtualizationManager.Instance.UiThreadExcecuteAction;
        var previousThrottle = VirtualizationManager.PropertySyncThrottleTime;

        // Materialisation has to happen on the UI thread, and a test has no dispatcher, so the action
        // is run where it is raised. A throttle of zero pushes a change straight through instead of
        // batching it, which is what makes the write observable as soon as the cell is edited.
        VirtualizationManager.Instance.UiThreadExcecuteAction = action =>
        {
            action();
            return Task.CompletedTask;
        };

        VirtualizationManager.PropertySyncThrottleTime = TimeSpan.Zero;

        try
        {
            var row = new RowEntity { Id = 1 };
            row.Cells.Add(NewCell(1, "before"));

            var dataSource = new RecordingRowDataSource([row]);

            var managed = await dataSource.GetViewModelAsync(entity => entity.Id == 1);

            Assert.NotNull(managed);

            managed.Cells[0].Value = "after";

            var update = Assert.Single(dataSource.Updates);

            Assert.Equal(1, update.RowId);
            Assert.Equal("after", update.Value);
        }
        finally
        {
            VirtualizationManager.Instance.UiThreadExcecuteAction = previousExecute;
            VirtualizationManager.PropertySyncThrottleTime = previousThrottle;
        }
    }
}
