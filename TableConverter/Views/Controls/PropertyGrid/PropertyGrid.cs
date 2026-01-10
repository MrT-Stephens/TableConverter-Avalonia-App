using System.ComponentModel;

namespace TableConverter.Views.Controls.PropertyGrid;

public class PropertyGrid : SukiUI.Controls.PropertyGrid
{
    protected override void SetItem(INotifyPropertyChanged? item)
    {
        if (item is null)
        {
            return;
        }

        Instance = new InstanceViewModel(item);
    }
}