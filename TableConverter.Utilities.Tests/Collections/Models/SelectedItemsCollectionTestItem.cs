using System.ComponentModel;

namespace TableConverter.Utilities.Tests.Collections.Models;

public class SelectedItemsCollectionTestItem(string name) : INotifyPropertyChanged
{
    private string _Name = name;

    public string Name
    {
        get => _Name;
        set
        {
            if (_Name == value) return;
            _Name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}