using Avalonia.Controls;

namespace TableConverter.Interfaces;

public interface IToolSettings
{
    public Dock Position { get; set; }
    
    public double Size { get; set; }
}