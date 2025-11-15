using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Reflection;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.ViewModels;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Tools;
using TableConverter.ViewModels.Workspaces;
using TableConverter.Views;
using TableConverter.Views.Documents;
using TableConverter.Views.Tools;
using TableConverter.Views.Workspaces;
using TableDataViewModel = TableConverter.ViewModels.Documents.TableDataViewModel;

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
            
            var viewModel = provider.GetRequiredService<MainWindowViewModel>()
                ?? throw new InvalidOperationException("Failed to create main window view model");
            
            window.DataContext = viewModel;

            window.Hosts.Add(new SukiToastHost()
            {
                Manager = provider.GetRequiredService<ISukiToastManager>()
                    ?? throw new InvalidOperationException("Failed to create toast manager"),
            });

            window.Hosts.Add(new SukiDialogHost()
            {
                Manager = provider.GetRequiredService<ISukiDialogManager>()
                    ?? throw new InvalidOperationException("Failed to create dialog manager"),
            });

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IViewsCollection ConfigureViews(ServiceCollection services)
    {
        var views = new ViewsCollection()
            // Workspaces
            .AddView<BaseWorkspaceEditorView, TableWorkspaceEditorViewModel>(services)
            .AddView<BaseWorkspaceEditorView, DataGenerationWorkspaceViewModel>(services)
            // Documents
            .AddView<TableDataView, TableDataViewModel>(services)
            .AddView<DataGenerationSchemeView, DataGenerationSchemaViewModel>(services)
            .AddView<MarkdownView, MarkdownViewModel>(services)
            // Tools
            .AddView<TableUtilitiesView, TableUtilitiesViewModel>(services);
        
        return views;
    }

    private static ServiceProvider ConfigureServices(ServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Main Display Window
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();

        // Custom Services
        services.AddSingleton<IConverterTypes, ConverterTypes>();
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        services.AddSingleton<IFilesDialogManager, FilesDialogManager>();
        services.AddSingleton<IEventManager, EventManager>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        
        // Register Command Handlers
        services.RegisterCommandHandlers(assembly);

        return services.BuildServiceProvider();
    }
}