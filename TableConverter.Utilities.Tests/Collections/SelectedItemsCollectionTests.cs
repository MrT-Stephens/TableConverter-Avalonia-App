using System.Collections;
using System.Collections.Specialized;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Tests.Collections.Models;

namespace TableConverter.Utilities.Tests.Collections;

public class SelectedItemsCollectionTests
{
    [Fact]
    public void Add_ShouldAddItem_AndRaiseCollectionAndPropertyChanged()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("one");

        NotifyCollectionChangedEventArgs? collectionArgs = null;
        string? propertyName = null;

        sut.CollectionChanged += (_, e) => collectionArgs = e;
        sut.PropertyChanged += (_, e) => propertyName = e.PropertyName;

        sut.Add(item);

        Assert.Single(sut);
        Assert.True(((IList)sut).Contains(item));
        Assert.NotNull(collectionArgs);
        Assert.Equal(NotifyCollectionChangedAction.Add, collectionArgs!.Action);
        Assert.Contains(item, collectionArgs.NewItems!.Cast<object>());
        Assert.Equal(nameof(sut.Count), propertyName);
    }

    [Fact]
    public void Add_DuplicateShouldBeIgnored()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("dup");

        sut.Add(item);
        sut.Add(item);

        Assert.Single(sut);
    }

    [Fact]
    public void Remove_ShouldRemoveItem_AndRaiseEvents()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("toRemove");

        NotifyCollectionChangedEventArgs? collectionArgs = null;
        string? propertyName = null;

        sut.Add(item);

        sut.CollectionChanged += (_, e) => collectionArgs = e;
        sut.PropertyChanged += (_, e) => propertyName = e.PropertyName;

        sut.Remove(item);

        Assert.Empty(sut);
        Assert.NotNull(collectionArgs);
        Assert.Equal(NotifyCollectionChangedAction.Remove, collectionArgs!.Action);
        Assert.Contains(item, collectionArgs.OldItems!.Cast<object>());
        Assert.Equal(nameof(sut.Count), propertyName);
    }

    [Fact]
    public void Clear_ShouldEmptyCollection_AndRaiseReset()
    {
        var sut = new SelectedItemsCollection();
        sut.Add(new SelectedItemsCollectionTestItem("a"));
        sut.Add(new SelectedItemsCollectionTestItem("b"));

        NotifyCollectionChangedEventArgs? args = null;
        sut.CollectionChanged += (_, e) => args = e;

        sut.Clear();

        Assert.Empty(sut);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args!.Action);
    }

    [Fact]
    public void TypedRetrieval_ShouldReturnOnlyRequestedType()
    {
        var sut = new SelectedItemsCollection();
        var t1 = new SelectedItemsCollectionTestItem("t1");
        var t2 = new SelectedItemsCollectionTestItem("t2");
        var other = new SelectedItemsCollectionOtherItem(42);

        sut.Add(t1);
        sut.Add(other);
        sut.Add(t2);

        var tests = sut.Get<SelectedItemsCollectionTestItem>().ToList();
        Assert.Equal(2, tests.Count);
        Assert.Contains(t1, tests);
        Assert.Contains(t2, tests);

        var single = sut.GetSingle<SelectedItemsCollectionOtherItem>();
        Assert.NotNull(single);
        Assert.Equal(other.Value, single!.Value);

        var singleByType = sut.GetSingle(typeof(SelectedItemsCollectionTestItem));
        Assert.Equal(t1, singleByType);
    }

    [Fact]
    public void RemoveAll_ByType_ShouldRemoveAllInstancesOfType()
    {
        var sut = new SelectedItemsCollection();
        var t1 = new SelectedItemsCollectionTestItem("x");
        var t2 = new SelectedItemsCollectionTestItem("y");
        var other = new SelectedItemsCollectionOtherItem(7);

        sut.Add(t1);
        sut.Add(other);
        sut.Add(t2);

        sut.RemoveAll<SelectedItemsCollectionTestItem>();

        Assert.Single(sut);
        Assert.Contains(other, sut.Get<SelectedItemsCollectionOtherItem>());
        Assert.Empty(sut.Get<SelectedItemsCollectionTestItem>());
    }

    [Fact]
    public void IList_Insert_ShouldInsertAtIndex_AndProvideIndexInCollectionChanged()
    {
        var sut = new SelectedItemsCollection();
        var list = (IList)sut;

        var a = new SelectedItemsCollectionTestItem("a");
        var b = new SelectedItemsCollectionTestItem("b");

        NotifyCollectionChangedEventArgs? args = null;
        sut.CollectionChanged += (_, e) => args = e;

        list.Add(a);
        list.Insert(0, b);

        Assert.Equal(2, sut.Count);
        Assert.Equal(b, sut[0]);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Add, args!.Action);
        Assert.Equal(0, args.NewStartingIndex);
    }

    [Fact]
    public void Indexer_Set_ShouldReplaceAndRaiseReset()
    {
        var sut = new SelectedItemsCollection();
        
        var a = new SelectedItemsCollectionTestItem("A");
        var b = new SelectedItemsCollectionTestItem("B");
        var replacement = new SelectedItemsCollectionTestItem("R");

        sut.Add(a);
        sut.Add(b);

        NotifyCollectionChangedEventArgs? args = null;
        sut.CollectionChanged += (_, e) => args = e;

        sut[0] = replacement;

        Assert.Equal(2, sut.Count);
        Assert.Equal(replacement, sut[0]);
        Assert.Equal(b, sut[1]);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args!.Action);
    }

    [Fact]
    public void ItemChanged_ShouldFire_WhenItemPropertyChanges()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("start");

        object? changedItem = null;
        sut.ItemChanged += (_, e) => changedItem = e.Item;

        sut.Add(item);

        // mutate property
        item.Name = "changed";

        Assert.Equal(item, changedItem);
    }

    [Fact]
    public void ItemChanged_ShouldFire_WhenNestedCollectionChanges()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItemWithCollection();

        object? changedItem = null;
        sut.ItemChanged += (_, e) => changedItem = e.Item;

        sut.Add(item);

        // mutate nested collection - AttachItemEvents should register its CollectionChanged
        item.Children.Add("x");

        Assert.Equal(item, changedItem);
    }

    [Fact]
    public void Dispose_ShouldClear_AndRaiseReset()
    {
        var sut = new SelectedItemsCollection();
        sut.Add(new SelectedItemsCollectionTestItem("one"));

        NotifyCollectionChangedEventArgs? args = null;
        sut.CollectionChanged += (_, e) => args = e;

        sut.Dispose();

        Assert.Empty(sut);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args!.Action);
    }

    [Fact]
    public void IList_CopyTo_ShouldCopyArray()
    {
        var sut = new SelectedItemsCollection();
        
        var a = new SelectedItemsCollectionTestItem("1");
        var b = new SelectedItemsCollectionTestItem("2");
        
        sut.Add(a);
        sut.Add(b);

        var arr = new object[2];
        
        ((ICollection)sut).CopyTo(arr, 0);

        Assert.Equal(a, arr[0]);
        Assert.Equal(b, arr[1]);
    }
}