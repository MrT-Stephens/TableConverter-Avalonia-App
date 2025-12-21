using System.ComponentModel;

namespace TableConverter.Utilities.Virtualisation;

public class DataWrapper<T>(int index) : INotifyPropertyChanged where T : class
{
    #region Fields and Properties
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private T? _Data;

    public int Index => index;

    public int ItemNumber => index + 1;

    public bool IsLoading => Data is null;

    public T? Data
    {
        get => _Data;
        internal set
        {
            _Data = value;
            OnPropertyChanged(nameof(Data));
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    public bool IsInUse => PropertyChanged != null;
    
    #endregion

    #region Methods
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #endregion
}