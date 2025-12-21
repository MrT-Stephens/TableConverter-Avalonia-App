using TableConverter.Utilities.Virtualisation;
using TableConverter.Utilities.Virtualisation.Interfaces;

namespace TableConverter.Utilities.Tests;

public class VirtualisationCollectionTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestItemsProvider : IItemsProvider<TestItem>
    {
        public List<TestItem> Items { get; } = new();

        public int FetchCount() => Items.Count;

        public IList<TestItem> FetchRange(int startIndex, int pageCount, out int overallCount)
        {
            overallCount = Items.Count;
            return Items.Skip(startIndex).Take(pageCount).ToList();
        }

        public void UpdateItem(int index, TestItem item) => Items[index] = item;

        public void InsertItem(int index, TestItem item) => Items.Insert(index, item);

        public void RemoveItem(int index) => Items.RemoveAt(index);

        public void ClearItems() => Items.Clear();
    }

    [Fact]
    public void Add_ShouldAddItemAndIncrementCount()
    {
        var provider = new TestItemsProvider();
        var collection = new VirtualisationCollection<TestItem>(provider, 10);
        var item = new TestItem { Name = "Test" };
        var wrapper = new DataWrapper<TestItem>(0) { Data = item };

        collection.Add(wrapper);

        Assert.Single(collection);
        Assert.Single(provider.Items);
        Assert.Equal("Test", provider.Items[0].Name);
    }

    [Fact]
    public void Insert_ShouldInsertItemAtIndex()
    {
        var provider = new TestItemsProvider();
        provider.Items.Add(new TestItem { Name = "Existing" });
        var collection = new VirtualisationCollection<TestItem>(provider, 10);

        // Force count initialization before insert
        _ = collection.Count;

        var item = new TestItem { Name = "Inserted" };
        var wrapper = new DataWrapper<TestItem>(0) { Data = item };

        collection.Insert(0, wrapper);

        Assert.Equal(2, provider.FetchCount());
        Assert.Equal("Inserted", provider.Items[0].Name);
        Assert.Equal("Existing", provider.Items[1].Name);
    }

    [Fact]
    public void RemoveAt_ShouldRemoveItemAtIndex()
    {
        var provider = new TestItemsProvider();
        provider.Items.Add(new TestItem { Name = "Item1" });
        provider.Items.Add(new TestItem { Name = "Item2" });
        var collection = new VirtualisationCollection<TestItem>(provider, 10);

        // Force count initialization before remove
        _ = collection.Count;

        collection.RemoveAt(0);

        Assert.Equal(1, provider.FetchCount());
        Assert.Equal("Item2", provider.Items[0].Name);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var provider = new TestItemsProvider();
        provider.Items.Add(new TestItem { Name = "Item1" });
        var collection = new VirtualisationCollection<TestItem>(provider, 10);

        collection.Clear();

        Assert.Equal(0, provider.FetchCount());
        Assert.Empty(provider.Items);
    }

    [Fact]
    public void Indexer_Set_ShouldUpdateItem()
    {
        var provider = new TestItemsProvider();
        provider.Items.Add(new TestItem { Name = "Old" });
        var collection = new VirtualisationCollection<TestItem>(provider, 10);
        var newItem = new TestItem { Name = "New" };
        var wrapper = new DataWrapper<TestItem>(0) { Data = newItem };

        collection[0] = wrapper;

        Assert.Equal("New", provider.Items[0].Name);
    }

    [Fact]
    public void IsReadOnly_ShouldBeFalse()
    {
        var provider = new TestItemsProvider();
        var collection = new VirtualisationCollection<TestItem>(provider, 10);

        Assert.False(collection.IsReadOnly);
    }
}
