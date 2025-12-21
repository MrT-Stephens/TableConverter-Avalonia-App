using System.Collections;
using TableConverter.Utilities.Virtualisation.Interfaces;

namespace TableConverter.Utilities.Virtualisation;

/// <summary>
/// Specialized list implementation that provides data virtualization. The collection is divided up into pages,
/// and pages are dynamically fetched from the IItemsProvider when required. Stale pages are removed after a
/// configurable period of time.
/// Intended for use with large collections on a network or disk resource that cannot be instantiated locally
/// due to memory consumption or fetch latency.
/// </summary>
public class VirtualisationCollection<T> : IList<DataWrapper<T>>, IList where T : class
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualisationCollection{T}"/> class.
    /// </summary>
    /// <param name="itemsProvider">The item's provider.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageTimeout">The page timeout.</param>
    public VirtualisationCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
    {
        ItemsProvider = itemsProvider;
        PageSize = pageSize;
        PageTimeout = pageTimeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualisationCollection{T}"/> class.
    /// </summary>
    /// <param name="itemsProvider">The item's provider.</param>
    /// <param name="pageSize">Size of the page.</param>
    public VirtualisationCollection(IItemsProvider<T> itemsProvider, int pageSize)
    {
        ItemsProvider = itemsProvider;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualisationCollection{T}"/> class.
    /// </summary>
    /// <param name="itemsProvider">The item's provider.</param>
    public VirtualisationCollection(IItemsProvider<T> itemsProvider)
    {
        ItemsProvider = itemsProvider;
    }

    #endregion

    #region ItemsProvider
    
    public IItemsProvider<T> ItemsProvider { get; }

    #endregion

    #region PageSize
    
    public int PageSize { get; } = 100;

    #endregion

    #region PageTimeout
    
    public long PageTimeout { get; } = 10000;

    #endregion

    #region IList<DataWrapper<T>>, IList

    #region Count

    private int _Count = -1;
    
    public int Count
    {
        get
        {
            if (_Count == -1)
            {
                _Count = 0;
                LoadCount();
            }
            
            return _Count;
        }
        protected set => _Count = value;
    }

    #endregion

    #region Indexer
    
    public DataWrapper<T> this[int index]
    {
        get
        {
            // determine which page and offset within page
            var pageIndex = index / PageSize;
            var pageOffset = index % PageSize;

            // request primary page
            RequestPage(pageIndex);

            // if accessing upper 50% then request next page
            if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                RequestPage(pageIndex + 1);

            // if accessing lower 50% then request prev page
            if (pageOffset < PageSize / 2 && pageIndex > 0)
                RequestPage(pageIndex - 1);

            // remove stale pages
            CleanUpPages();

            // return requested item
            return _Pages[pageIndex].Items[pageOffset];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (value?.Data == null)
                throw new ArgumentNullException(nameof(value));

            // Update in the provider
            ItemsProvider.UpdateItem(index, value.Data);

            // Update in the cached page if it exists
            var pageIndex = index / PageSize;
            var pageOffset = index % PageSize;
            if (_Pages.TryGetValue(pageIndex, out var page))
            {
                page.Items[pageOffset] = value;
            }
        }
    }

    object? IList.this[int index]
    {
        get => this[index];
        set
        {
            if (value is DataWrapper<T> wrapper)
                this[index] = wrapper;
            else
                throw new ArgumentException("Value must be of type DataWrapper<T>", nameof(value));
        }
    }

    #endregion

    #region IEnumerator<DataWrapper<T>>, IEnumerator

    public IEnumerator<DataWrapper<T>> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Add

    public virtual void Add(DataWrapper<T> item)
    {
        if (item?.Data == null)
            throw new ArgumentNullException(nameof(item));

        ItemsProvider.InsertItem(Count, item.Data);
        Count++;
        EmptyCache();
    }

    int IList.Add(object? value)
    {
        if (value is DataWrapper<T> wrapper)
        {
            Add(wrapper);
            return Count - 1;
        }
        throw new ArgumentException("Value must be of type DataWrapper<T>", nameof(value));
    }

    #endregion

    #region Contains

    bool IList.Contains(object? value)
    {
        return Contains((DataWrapper<T>?)value);
    }
    
    public bool Contains(DataWrapper<T>? item)
    {
        return item != null && _Pages.Values.Any(page => page.Items.Contains(item));
    }

    #endregion

    #region Clear
    
    public virtual void Clear()
    {
        ItemsProvider.ClearItems();
        Count = 0;
        EmptyCache();
    }

    #endregion

    #region IndexOf

    int IList.IndexOf(object? value)
    {
        return IndexOf((DataWrapper<T>?)value);
    }
    
    public int IndexOf(DataWrapper<T>? item)
    {
        if (item != null)
        {
            foreach (var keyValuePair in _Pages)
            {
                var indexWithinPage = keyValuePair.Value.Items.IndexOf(item);
                
                if (indexWithinPage != -1)
                {
                    return PageSize * keyValuePair.Key + indexWithinPage;
                }
            }
        }
        
        return -1;
    }

    #endregion

    #region Insert
    
    public virtual void Insert(int index, DataWrapper<T>? item)
    {
        if (item?.Data == null)
            throw new ArgumentNullException(nameof(item));

        ItemsProvider.InsertItem(index, item.Data);
        Count++;
        EmptyCache();
    }

    void IList.Insert(int index, object? value)
    {
        Insert(index, (DataWrapper<T>?)value);
    }

    #endregion

    #region Remove
    
    public virtual void RemoveAt(int index)
    {
        ItemsProvider.RemoveItem(index);
        Count--;
        EmptyCache();
    }

    void IList.Remove(object? value)
    {
        if (value is DataWrapper<T> item)
        {
            Remove(item);
        }
    }
    
    public virtual bool Remove(DataWrapper<T> item)
    {
        var index = IndexOf(item);
        if (index != -1)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    #endregion

    #region CopyTo
    
    public void CopyTo(DataWrapper<T>[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    void ICollection.CopyTo(Array array, int index)
    {
        throw new NotSupportedException();
    }

    #endregion

    #region Misc
    
    public object SyncRoot => this;
    
    public bool IsSynchronized => false;
    
    public bool IsReadOnly => false;
    
    public bool IsFixedSize => false;

    #endregion

    #endregion

    #region Paging

    private Dictionary<int, DataPage<T>> _Pages = new();

    /// <summary>
    /// Cleans up any stale pages that have not been accessed in the period dictated by PageTimeout.
    /// </summary>
    public void CleanUpPages()
    {
        var keys = _Pages.Keys.ToArray();
        
        foreach (var key in keys)
        {
            // page 0 is a special case, since WPF ItemsControl access the first item frequently
            if (key != 0 && (DateTime.Now - _Pages[key].TouchTime).TotalMilliseconds > PageTimeout)
            {
                var removePage = true;

                if (_Pages.TryGetValue(key, out var page))
                {
                    removePage = !page.IsInUse;
                }

                if (removePage)
                {
                    _Pages.Remove(key);
                }
            }
        }
    }
    
    protected virtual void RequestPage(int pageIndex)
    {
        if (!_Pages.TryGetValue(pageIndex, out var currentPage))
        {
            // Create a page of empty data wrappers.
            var pageLength = Math.Min(PageSize, Count - pageIndex * PageSize);
            var page = new DataPage<T>(pageIndex * PageSize, pageLength);
            _Pages.Add(pageIndex, page);
            LoadPage(pageIndex, pageLength);
        }
        else
        {
            currentPage.TouchTime = DateTime.Now;
        }
    }
    
    protected virtual void PopulatePage(int pageIndex, IList<T> dataItems)
    {
        if (_Pages.TryGetValue(pageIndex, out var page))
        {
            page.Populate(dataItems);
        }
    }
    
    protected void EmptyCache()
    {
        _Pages = new Dictionary<int, DataPage<T>>();
    }

    #endregion

    #region Load methods
    
    protected virtual void LoadCount()
    {
        Count = FetchCount();
    }
    
    protected virtual void LoadPage(int pageIndex, int pageLength)
    {
        PopulatePage(pageIndex, FetchPage(pageIndex, pageLength, out var count));
        Count = count;
    }

    #endregion

    #region Fetch methods
    
    protected IList<T> FetchPage(int pageIndex, int pageLength, out int count)
    {
        return ItemsProvider.FetchRange(pageIndex * PageSize, pageLength, out count);
    }
    
    protected int FetchCount()
    {
        return ItemsProvider.FetchCount();
    }

    #endregion
}