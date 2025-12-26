using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Data;

namespace TableConverter.Common
{
    public class VirtualisingCollectionView : IDataGridCollectionView, IList, INotifyPropertyChanged
    {
        public static IDataGridCollectionView GetDefaultView<T>(ObservableCollection<T> coll)
        {
            return new VirtualisingCollectionView(coll);
        }
        
        #region Private Fields

        private const int CachedCountNotComputed = -1;
        private int _CachedCount = CachedCountNotComputed;
        private CultureInfo _Culture;
        private object? _CurrentItem;
        private int _CurrentPosition = -1;
        private Predicate<object>? _Filter;
        private bool? _CachedIsEmpty;
        private bool _IsCurrentAfterLast;
        private bool _IsCurrentBeforeFirst;
        private readonly IEnumerable _sourceCollection;
        private bool _IsRefreshDeferred;

        #endregion

        #region Public Constructors

        public VirtualisingCollectionView(IEnumerable collection)
        {
            _sourceCollection = collection 
                ?? throw new ArgumentNullException(nameof(collection));

            var enumerator = collection.GetEnumerator();
            
            if (enumerator.MoveNext())
            {
                _CurrentItem = enumerator.Current;
                _CurrentPosition = 0;
                _IsCurrentAfterLast = false;
                _IsCurrentBeforeFirst = false;
            }
            else
            {
                _IsCurrentAfterLast = true;
                _IsCurrentBeforeFirst = true;
            }

            if (collection is INotifyCollectionChanged incc)
            {
                incc.CollectionChanged += OnCollectionChanged;
            }
        }

        #endregion

        #region Public Properties

        public virtual bool CanFilter => true;

        public virtual bool CanGroup => false;

        public virtual bool CanSort => true;

        public virtual IComparer Comparer => null;

        public virtual int Count
        {
            get
            {
                if (_CachedCount == CachedCountNotComputed)
                {
                    _CachedCount = 0;
                    
                    if (_sourceCollection is IList lst)
                    {
                        _CachedCount = lst.Count;
                    }
                    else
                    {
                        var enumerator = _sourceCollection.GetEnumerator();
                        
                        while (enumerator.MoveNext())
                        {
                            _ = enumerator.Current;
                            _CachedCount++;
                        }
                    }
                }
                
                return _CachedCount;
            }
        }
        
        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        public virtual CultureInfo Culture
        {
            get => _Culture;
            set => _Culture = value;
        }

        public virtual object CurrentItem
        {
            get
            {
                VerifyRefreshNotDeferred();
                return _CurrentItem!;
            }
        }

        public virtual int CurrentPosition
        {
            get
            {
                VerifyRefreshNotDeferred();
                return _CurrentPosition;
            }
        }

        public virtual Predicate<object>? Filter
        {
            get => _Filter;
            set
            {
                if (!CanFilter)
                    throw new NotSupportedException();
                
                _Filter = value;
                RefreshOrDefer();
            }
        }

        bool IDataGridCollectionView.IsGrouping => false;
        int IDataGridCollectionView.GroupingDepth => 0;
        IAvaloniaReadOnlyList<object> IDataGridCollectionView.Groups => new AvaloniaList<object>();

        public virtual bool IsCurrentAfterLast => _IsCurrentAfterLast;

        public virtual bool IsCurrentBeforeFirst => _IsCurrentBeforeFirst;

        public virtual bool IsEmpty
        {
            get
            {
                if (!_CachedIsEmpty.HasValue)
                {
                    var enumerator = _sourceCollection.GetEnumerator();
                    _CachedIsEmpty = !enumerator.MoveNext();
                }
                
                return _CachedIsEmpty.Value;
            }
        }

        public virtual bool NeedsRefresh => true;

        DataGridSortDescriptionCollection IDataGridCollectionView.SortDescriptions => [];

        public virtual IEnumerable SourceCollection => _sourceCollection;

        #endregion

        #region Protected Properties

        protected bool IsCurrentInSync => false;

        protected bool IsDynamic => false;

        protected bool IsRefreshDeferred => _IsRefreshDeferred;

        protected bool UpdatedOutsideDispatcher => false;

        Func<object, bool> IDataGridCollectionView.Filter
        {
            get => _Filter is null ? _ => true : new Func<object, bool>(o => _Filter!(o));
            set
            {
                _Filter = value is null ? null : new Predicate<object>(o => value(o));
                RefreshOrDefer();
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        object? IList.this[int index]
        {
            get => GetItemAt(index);
            set
            {
                if (_sourceCollection is IList list)
                {
                    var oldItem = list[index];
                    list[index] = value!;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
                }
                else
                {
                    throw new NotSupportedException("Underlying collection is not an IList and cannot set by index.");
                }
            }
        }

        #endregion

        #region Public Methods

        public virtual bool Contains(object? item)
        {
            if (item == null)
                return false;
            
            if (!PassesFilter(item))
                return false;

            return _sourceCollection.Cast<object>().Contains(item);
        }

        public virtual IDisposable DeferRefresh()
        {
            return new DeferHelper(this);
        }

        protected virtual object GetItemAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_sourceCollection is IList l)
            {
                var item = l[index];
                if (item is not null )
                {
                    return item;
                }
            }

            var currentCollectionItemIndex = 0;
            
            foreach (var collectionItem in _sourceCollection)
            {
                if (currentCollectionItemIndex == index)
                    return collectionItem;
                
                currentCollectionItemIndex++;
            }
            
            throw new IndexOutOfRangeException();
        }

        public virtual int IndexOf(object? item)
        {
            if (_sourceCollection is IList sc)
            {
                int idx = sc.IndexOf(item);
                if (idx > -1)
                {
                    return idx;
                }
            }

            var currentCollectionItemIndex = 0;
            foreach (var collectionItem in _sourceCollection)
            {
                if (!PassesFilter(collectionItem))
                    continue;
                
                if (Equals(item, collectionItem))
                    return currentCollectionItemIndex;
                
                currentCollectionItemIndex++;
            }
            
            return -1;
        }

        public virtual bool MoveCurrentTo(object item)
        {
            VerifyRefreshNotDeferred();
            
            if (Equals(_CurrentItem, item))
                return true;

            if (_sourceCollection is IList sc)
            {
                int idx = sc.IndexOf(item);
                if (idx > -1)
                {
                    return MoveCurrentToPosition(idx);
                }
            }

            var currentCollectionItemIndex = 0;
            foreach (var collectionItem in _sourceCollection)
            {
                if (Equals(item, collectionItem))
                    return MoveCurrentToPosition(currentCollectionItemIndex);
                
                currentCollectionItemIndex++;
            }
            
            return MoveCurrentToPosition(-1);
        }

        public virtual bool MoveCurrentToFirst()
        {
            VerifyRefreshNotDeferred();
            
            _CurrentPosition = 0;
            var enumerator = _sourceCollection.GetEnumerator();
            
            if (enumerator.MoveNext())
            {
                _CurrentItem = enumerator.Current;
                _IsCurrentAfterLast = false;
                _IsCurrentBeforeFirst = false;
                return true;
            }

            _CurrentItem = null;
            _IsCurrentAfterLast = true;
            _IsCurrentBeforeFirst = true;
            return false;
        }

        public virtual bool MoveCurrentToLast()
        {
            VerifyRefreshNotDeferred();
            _CurrentItem = null;
            _CurrentPosition = -1;
            
            foreach (var item in _sourceCollection)
            {
                _CurrentItem = item;
                _CurrentPosition++;
            }
            
            _IsCurrentAfterLast = _CurrentPosition == -1;
            _IsCurrentBeforeFirst = _IsCurrentAfterLast;
            
            return !_IsCurrentAfterLast;
        }

        public virtual bool MoveCurrentToNext()
        {
            VerifyRefreshNotDeferred();
            return !_IsCurrentAfterLast && MoveCurrentToPosition(_CurrentPosition + 1);
        }

        public virtual event EventHandler<DataGridCurrentChangingEventArgs> CurrentChanging;

        protected void OnCurrentChanging()
        {
            _CurrentPosition = -1;
            OnCurrentChanging(new DataGridCurrentChangingEventArgs(false));
        }

        protected virtual void OnCurrentChanging(DataGridCurrentChangingEventArgs args)
        {
            CurrentChanging?.Invoke(this, args);
        }

        public virtual bool MoveCurrentToPosition(int position)
        {
            VerifyRefreshNotDeferred();
            DataGridCurrentChangingEventArgs e;
            _IsCurrentAfterLast = false;
            _IsCurrentBeforeFirst = false;
            if (position < 0)
            {
                e = new DataGridCurrentChangingEventArgs();
                OnCurrentChanging(e);
                if (e.Cancel)
                    return true;
                _CurrentPosition = -1;
                _CurrentItem = null;
                _IsCurrentBeforeFirst = true;
                OnCurrentChanged();
                return false;
            }
            
            if (_sourceCollection is IList sc)
            {
                var item = sc[position];
                if (item is not null)
                {
                    e = new DataGridCurrentChangingEventArgs();
                    OnCurrentChanging(e);
                    if (e.Cancel)
                        return true;
                    _CurrentPosition = position;
                    _CurrentItem = item;
                    OnCurrentChanged();
                    return true;
                }
            }

            _CurrentPosition = 0;
            foreach (var item in _sourceCollection)
            {
                if (_CurrentPosition == position)
                {
                    e = new DataGridCurrentChangingEventArgs();
                    OnCurrentChanging(e);
                    if (e.Cancel)
                        return true;
                    _CurrentItem = item;
                    OnCurrentChanged();
                    return true;
                }
                _CurrentPosition++;
            }
            if (position <= _CurrentPosition)
            {
                e = new DataGridCurrentChangingEventArgs();
                OnCurrentChanging(e);
                if (e.Cancel)
                    return true;
            }
            _CurrentItem = null;
            _IsCurrentAfterLast = true;
            if (position > _CurrentPosition)
                throw new ArgumentOutOfRangeException(nameof(position));
            OnCurrentChanged();
            return false;
        }

        public virtual bool MoveCurrentToPrevious()
        {
            VerifyRefreshNotDeferred();
            return !_IsCurrentBeforeFirst && MoveCurrentToPosition(_CurrentPosition - 1);
        }

        public virtual bool PassesFilter(object item)
        {
            return _Filter == null || _Filter(item);
        }

        public virtual void Refresh()
        {
            RefreshOverride();
        }

        #endregion

        #region Protected Methods

        protected void ClearChangeLog()
        {
            //WDTDH
        }

        protected virtual IEnumerator GetEnumerator()
        {
            return SourceCollection.GetEnumerator();
        }

        protected bool OKToChangeCurrent()
        {
            //WDTDH
            return true;
        }

        protected virtual void OnBeginChangeLogging(NotifyCollectionChangedEventArgs args)
        {
            //LAMESPEC?: Documentation says it should throw ArgumentNullException if args is null. It seems it does not.
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            //FIXME
            //LAMESPEC?
            //if (Dispatcher.Thread != Thread.CurrentThread)
            //    throw new NotSupportedException("This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread.");
            ProcessCollectionChanged(args);
        }

        protected virtual void OnCurrentChanged()
        {
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            // Invalidate cached values
            _CachedCount = CachedCountNotComputed;
            _CachedIsEmpty = null;

            // Adjust current position when items removed before current
            if (args.Action == NotifyCollectionChangedAction.Remove && args.OldStartingIndex >= 0)
            {
                if (_CurrentPosition >= args.OldStartingIndex)
                {
                    _CurrentPosition = Math.Max(-1, _CurrentPosition - (args.OldItems?.Count ?? 1));
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                // Reset current if out of range
                if (_CurrentPosition >= Count)
                {
                    _CurrentPosition = Count - 1;
                    _CurrentItem = _CurrentPosition >= 0 ? GetItemAt(_CurrentPosition) : null;
                }
            }

            OnCollectionChanged(args);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEmpty)));
        }

        protected void RefreshOrDefer()
        {
            if (_IsRefreshDeferred)
                return;
            Refresh();
        }

        protected virtual void RefreshOverride()
        {
            // Notify that the view has changed (e.g., filter updated)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEmpty)));
        }

        protected void SetCurrent(object newItem, int newPosition)
        {
            _CurrentItem = newItem;
            _CurrentPosition = newPosition;
            _IsCurrentBeforeFirst = _CurrentPosition < 0;
            _IsCurrentAfterLast = _CurrentPosition >= Count;
        }

        #endregion

        #region Public Events

        public virtual event EventHandler CurrentChanged;

        #endregion

        #region Protected Events

        protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Explicit Interface Implementations

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged

        event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
        {
            add => CollectionChanged += value;
            remove => CollectionChanged -= value;
        }

        #endregion

        #region INotifyPropertyChanged

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        #endregion

        #endregion

        #region Private Methods

        void VerifyRefreshNotDeferred()
        {
            if (_IsRefreshDeferred)
                throw new InvalidOperationException("Cannot change or check the contents or Current position of CollectionView while Refresh is being deferred.");
        }

        string IDataGridCollectionView.GetGroupingPropertyNameAtDepth(int level)
        {
            return string.Empty;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            int i = index;
            foreach (var item in _sourceCollection)
            {
                array.SetValue(item, i++);
            }
        }

        int IList.Add(object? value)
        {
            if (_sourceCollection is IList list)
            {
                int idx = list.Add(value);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, idx));
                return idx;
            }
            throw new NotSupportedException("Underlying collection does not support Add");
        }

        void IList.Clear()
        {
            if (_sourceCollection is IList list)
            {
                list.Clear();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return;
            }
            throw new NotSupportedException("Underlying collection does not support Clear");
        }

        void IList.Insert(int index, object? value)
        {
            if (_sourceCollection is IList list)
            {
                list.Insert(index, value);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
                return;
            }
            throw new NotSupportedException("Underlying collection does not support Insert");
        }

        void IList.Remove(object? value)
        {
            if (_sourceCollection is IList list)
            {
                int idx = list.IndexOf(value);
                if (idx >= 0)
                {
                    var old = list[idx];
                    list.RemoveAt(idx);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, idx));
                }
                return;
            }
            throw new NotSupportedException("Underlying collection does not support Remove");
        }

        void IList.RemoveAt(int index)
        {
            if (_sourceCollection is IList list)
            {
                var old = list[index];
                list.RemoveAt(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
                return;
            }
            throw new NotSupportedException("Underlying collection does not support RemoveAt");
        }

        #endregion

        #region Private Classes

        private class DeferHelper : IDisposable
        {
            private readonly VirtualisingCollectionView _owner;

            public DeferHelper(VirtualisingCollectionView owner)
            {
                _owner = owner;
                owner._IsRefreshDeferred = true;
            }

            public void Dispose()
            {
                _owner._IsRefreshDeferred = false;
                _owner.Refresh();
            }
        }

        #endregion
    }
}
