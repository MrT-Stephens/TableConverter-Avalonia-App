using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace TableConverter.Interfaces;

public interface IInitializeControls
{
    public Collection<Control> Controls { get; set; }

    public void InitializeControls();
}