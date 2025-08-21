using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace TableConverter.Interfaces;

public interface ITopLevelAware
{
    public TopLevel? GetTopLevel()
    {
        return Application.Current is
            { ApplicationLifetime: IClassicDesktopStyleApplicationLifetime { MainWindow: { } window } }
            ? TopLevel.GetTopLevel(window)
            : null;
    }
}