using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TableConverter.Utilities.Tests.Collections.Models;

public class SelectedItemsCollectionTestItemWithCollection : INotifyPropertyChanged
{
    private ObservableCollection<string> _Children = [];
    public ObservableCollection<string> Children
    {
        get => _Children;
        set
        {
            if (_Children == value) return;
            _Children = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}