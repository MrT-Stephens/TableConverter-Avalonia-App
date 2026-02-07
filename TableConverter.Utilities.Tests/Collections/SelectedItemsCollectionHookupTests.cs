using System.Collections;
using System.Collections.ObjectModel;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Tests.Collections.Models;

namespace TableConverter.Utilities.Tests.Collections;

public class SelectedItemsCollectionHookupTests
{
    [Fact]
    public void Add_AttachesPropertyChanged_AndRemoveDetaches()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("x");

        int calls = 0;
        sut.ItemChanged += (_, e) => calls++;

        sut.Add(item);
        item.Name = "a"; // should trigger
        Assert.Equal(1, calls);

        sut.Remove(item);
        item.Name = "b"; // should NOT trigger after remove
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Add_SameItemTwice_DoesNotDoubleSubscribe()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("dup");

        int calls = 0;
        sut.ItemChanged += (_, e) => calls++;

        sut.Add(item);
        item.Name = "a";
        Assert.Equal(1, calls);

        // Attempt duplicate add - collection should ignore duplicate
        sut.Add(item);
        item.Name = "b";
        Assert.Equal(2, calls); // only one handler invoked per change
    }

    [Fact]
    public void Indexer_Replace_UnsubscribesOld_AttachesNew()
    {
        var sut = new SelectedItemsCollection();
        var oldItem = new SelectedItemsCollectionTestItem("old");
        var other = new SelectedItemsCollectionTestItem("other");
        var newItem = new SelectedItemsCollectionTestItem("new");

        sut.Add(oldItem);
        sut.Add(other);

        int calls = 0;
        object? last = null;
        sut.ItemChanged += (_, e) =>
        {
            calls++;
            last = e.Item;
        };

        sut[0] = newItem; // replace oldItem with newItem

        oldItem.Name = "changedOld"; // should NOT trigger
        newItem.Name = "changedNew"; // should trigger once

        Assert.Equal(1, calls);
        Assert.Equal(newItem, last);
    }

    [Fact]
    public void IList_Insert_AttachesPropertyChanged()
    {
        var sut = new SelectedItemsCollection();
        var list = (IList)sut;
        var item = new SelectedItemsCollectionTestItem("i");

        int calls = 0;
        sut.ItemChanged += (_, e) => calls++;

        list.Insert(0, item); // insert should attach handlers
        item.Name = "changed";

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Dispose_UnsubscribesAllItemHandlers()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItem("one");

        int calls = 0;
        sut.ItemChanged += (_, e) => calls++;

        sut.Add(item);
        sut.Dispose();

        item.Name = "changed";
        Assert.Equal(0, calls);
    }

    [Fact]
    public void RemoveAll_ByType_DetachesHandlers()
    {
        var sut = new SelectedItemsCollection();
        var t1 = new SelectedItemsCollectionTestItem("t1");
        var t2 = new SelectedItemsCollectionTestItem("t2");
        var other = new SelectedItemsCollectionOtherItem(1);

        int calls = 0;
        sut.ItemChanged += (_, e) => calls++;

        sut.Add(t1);
        sut.Add(other);
        sut.Add(t2);

        sut.RemoveAll<SelectedItemsCollectionTestItem>();

        t1.Name = "a";
        t2.Name = "b";
        Assert.Equal(0, calls); // test items removed -> no notifications
    }

    [Fact]
    public void ReplacingNestedCollection_UpdatesCollectionChangedSubscription()
    {
        var sut = new SelectedItemsCollection();
        var item = new SelectedItemsCollectionTestItemWithCollection();

        int calls = 0;
        object? last = null;
        sut.ItemChanged += (_, e) =>
        {
            calls++;
            last = e.Item;
        };

        sut.Add(item);

        // initial children change should notify
        item.Children.Add("a");
        Assert.Equal(1, calls);
        Assert.Equal(item, last);

        // replace the Children collection
        var oldChildren = item.Children;
        var newChildren = new ObservableCollection<string>();
        item.Children = newChildren;

        // changes to a new collection should notify
        newChildren.Add("b");
        Assert.Equal(2, calls);

        // changes to an old collection should NOT notify after replacement
        oldChildren.Add("c");
        Assert.Equal(2, calls);
    }
}