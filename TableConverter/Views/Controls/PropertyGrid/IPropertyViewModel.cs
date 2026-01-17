using System;
using System.ComponentModel;

namespace TableConverter.Views.Controls.PropertyGrid
{
    public interface IPropertyViewModel : INotifyPropertyChanged, IDisposable
    {
        object? Value { get; set; }
    }
}