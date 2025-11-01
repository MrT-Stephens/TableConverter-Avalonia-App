using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Interfaces.OverlayService;
using TableConverter.Services;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels;
using TableConverter.Views;

namespace TableConverter;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // The Line below is needed to remove Avalonia data validation.
        // Without this line, you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);

            provider.RegisterCommandHandlers();

            DataTemplates.Add(new ViewLocator(views));

            var window = provider.GetRequiredService<MainWindowView>()
                ?? throw new InvalidOperationException("Failed to create main window");

            window.Content = views.CreateView<MainViewModel>(provider);

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IViewsCollection ConfigureViews(ServiceCollection services)
    {
        var views = new ViewsCollection()
            .AddView<MainView, MainViewModel>(services)
            .AddView<TableDataView, TableDataViewModel>(services);
        
        return views;
    }

    private static ServiceProvider ConfigureServices(ServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Window
        services.AddSingleton<MainWindowView>();

        // Custom Services
        services.AddSingleton<IPageNavigation, PageNavigation>();
        services.AddSingleton<IConverterTypes, ConverterTypes>();
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        services.AddSingleton<IFilesDialogManager, FilesDialogManager>();
        services.AddSingleton<IEventManager, EventManager>();
        services.AddSingleton<IOverlayService, OverlayService>();

        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        
        // Register Command Handlers
        services.RegisterCommandHandlers(assembly);

        return services.BuildServiceProvider();
    }
}