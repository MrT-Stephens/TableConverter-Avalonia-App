using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

namespace TableConverter.Browser;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static Task Main(string[] args)
    {
        return BuildAvaloniaApp()
            .StartBrowserAppAsync("out");
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        var app = AppBuilder.Configure<App>()
            .WithInterFont();
        
        return app;
    }
}