using System;
using System.ComponentModel;

namespace TableConverter.Views.Controls.PropertyGrid
{
    public interface IPropertyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo, IDisposable
    {
        object? Value { get; set; }
    }
}