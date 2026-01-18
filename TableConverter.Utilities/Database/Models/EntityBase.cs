using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Utilities.Database.Models;

public class EntityBase<T> : IEntityWithId<T>
{
    #region IEntityWithId Implementation

    private T _Id = default!;
    public T Id
    {
        get => _Id;
        set => SetField(ref _Id, value);
    }

    #endregion
    
    #region INotifyPropertyChanged Implementation
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<TProperty>(ref TProperty field, TProperty value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TProperty>.Default.Equals(field, value))
        {
            return false;
        }
        
        field = value;
        OnPropertyChanged(propertyName);
        
        return true;
    }
    
    #endregion
}