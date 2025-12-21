using System.Collections.Specialized;
using System.ComponentModel;
using TableConverter.Utilities.Virtualisation.Interfaces;

namespace TableConverter.Utilities.Virtualisation;

public class AsyncVirtualisationCollection<T>(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
    : VirtualisationCollection<T>(itemsProvider, pageSize, pageTimeout), INotifyCollectionChanged, INotifyPropertyChanged
    where T : class
{
    #region SynchronizationContext

    /// <summary>
    /// Gets the synchronization context used for UI-related operations. This is obtained as
    /// the current SynchronizationContext when the AsyncVirtualizingCollection is created.
    /// </summary>
#pragma warning disable CS8601 // Possible null reference assignment.
    protected SynchronizationContext SynchronizationContext { get; } = SynchronizationContext.Current;
#pragma warning restore CS8601 // Possible null reference assignment.

    #endregion

    #region INotifyCollectionChanged
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
    
    private void FireCollectionReset()
    {
        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        OnCollectionChanged(args);
    }

    #endregion

    #region INotifyPropertyChanged
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
    
    private void FirePropertyChanged(string propertyName)
    {
        var args = new PropertyChangedEventArgs(propertyName);
        OnPropertyChanged(args);
    }

    #endregion

    #region IsLoading

    private bool _IsLoading;
    
    public bool IsLoading
    {
        get => _IsLoading;
        set
        {
            if (value != _IsLoading)
            {
                _IsLoading = value;
                FirePropertyChanged(nameof(IsLoading));
            }
        }
    }

    private bool _IsInitializing;

    public bool IsInitializing
    {
        get => _IsInitializing;
        set
        {
            if (value != _IsInitializing)
            {
                _IsInitializing = value;
                FirePropertyChanged(nameof(IsInitializing));
            }
        }
    }

    #endregion

    #region Load overrides

    /// <summary>
    /// Asynchronously loads the count of items.
    /// </summary>
    protected override void LoadCount()
    {
        if (Count == 0)
        {
            IsInitializing = true;
        }

        ThreadPool.QueueUserWorkItem(LoadCountWork);
    }
    
    private void LoadCountWork(object? args)
    {
        var count = FetchCount();
        SynchronizationContext.Send(LoadCountCompleted, count);
    }
    
    protected virtual void LoadCountCompleted(object? args)
    {
        var newCount = (int)(args ?? throw new ArgumentNullException(nameof(args)));
        TakeNewCount(newCount);
        IsInitializing = false;
    }

    private void TakeNewCount(int newCount)
    {
        if (newCount != Count)
        {
            Count = newCount;
            EmptyCache();
            FireCollectionReset();
        }
    }
    
    protected override void LoadPage(int pageIndex, int pageLength)
    {
        IsLoading = true;
        ThreadPool.QueueUserWorkItem(LoadPageWork, new[] { pageIndex, pageLength });
    }
    
    private void LoadPageWork(object? state)
    {
        var args = (int[]?)state ?? throw new ArgumentNullException(nameof(state));
        var pageIndex = args[0];
        var pageLength = args[1];
        var dataItems = FetchPage(pageIndex, pageLength, out var overallCount);
        SynchronizationContext.Send(LoadPageCompleted, new object[] { pageIndex, dataItems, overallCount });
    }
    
    private void LoadPageCompleted(object? state)
    {
        var args = (object[]?)state ?? throw new ArgumentNullException(nameof(state));
        var pageIndex = (int)args[0];
        var dataItems = (IList<T>)args[1];
        var newCount = (int)args[2];
        TakeNewCount(newCount);
        PopulatePage(pageIndex, dataItems);
        IsLoading = false;
    }

    #endregion
}