using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.Interfaces;
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
            
            ConfigureCommands(provider);

            DataTemplates.Add(new ViewLocator(views));

            desktop.MainWindow = views.CreateView<MainWindowViewModel>(provider) as Window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IViewsCollection ConfigureViews(IServiceCollection services)
    {
        var views = new ViewsCollection()
            // Main Window
            .AddView<MainWindowView, MainWindowViewModel>(services);

        return views;
    }

    private static IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // Custom Services
        services.AddSingleton<IPageNavigation, PageNavigation>();
        services.AddSingleton<IConverterTypes, ConverterTypes>();
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        services.AddSingleton<IFilesDialogManager, FilesDialogManager>();
        
        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<ICommandHandlerAsync, AddFileCommandHandler>();
        
        // Misc Services
        services.AddSingleton<IEventManager, EventManager>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureCommands(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<ICommandManager>();
        
        // Register all command handlers
        provider.GetServices<ICommandHandler>()
            .ForEach(handler => manager.RegisterCommand(handler.CommandMetadata.Name, handler));

        // Register all async command handlers
        provider.GetServices<ICommandHandlerAsync>()
            .ForEach(handler => manager.RegisterCommandAsync(handler.CommandMetadata.Name, handler));
    }
}