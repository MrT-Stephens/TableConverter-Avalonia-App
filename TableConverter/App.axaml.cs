using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using TableConverter.ViewModels;
using TableConverter.Views;

namespace TableConverter;

public partial class App : Application
{
    public static Avalonia.ThemeManager.IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new Avalonia.ThemeManager.FluentThemeManager();

        ThemeManager.Initialize(this);

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        Services.ConverterHandlerService converter_handler_service = new Services.ConverterHandlerService();
        Services.TableDataConverterService table_data_converter_service = new Services.TableDataConverterService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(converter_handler_service, table_data_converter_service)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(converter_handler_service, table_data_converter_service)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
