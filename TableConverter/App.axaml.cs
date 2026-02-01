using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.IO;
using System.Reflection;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using ModelFlow.DataVirtualization;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.FileConverters.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Utilities;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Factories;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels;
using TableConverter.ViewModels.Dialogs;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Tools;
using TableConverter.ViewModels.Workspaces;
using TableConverter.Views;
using TableConverter.Views.Dialogs;
using TableConverter.Views.Documents;
using TableConverter.Views.Tools;
using TableConverter.Views.Workspaces;

namespace TableConverter;

public class App : Application
{
    public static readonly string AppStorageDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create),
        "TableConverter");
    
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
            VirtualizationManager.Instance.UiThreadExcecuteAction = a => 
                Dispatcher.UIThread.InvokeAsync(a).GetTask();
            
            DispatcherTimer.Run(() =>
            {
                VirtualizationManager.Instance.ProcessActions();
                return true;
            }, TimeSpan.FromMilliseconds(10), DispatcherPriority.Background);
            
            var services = new ServiceCollection();

            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);
            
            provider.RegisterCommandHandlers();
            provider.RegisterCommandError();

            DataTemplates.Add(new ViewLocator(views));

            var window = provider.GetRequiredService<MainWindowView>()
                ?? throw new InvalidOperationException("Failed to create main window");
            
            var viewModel = provider.GetRequiredService<MainWindowViewModel>()
                ?? throw new InvalidOperationException("Failed to create main window view model");
            
            window.DataContext = viewModel;

            window.Hosts.Add(new SukiToastHost
            {
                Manager = provider.GetRequiredService<ISukiToastManager>()
                    ?? throw new InvalidOperationException("Failed to create toast manager"),
            });

            window.Hosts.Add(new SukiDialogHost
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
            .AddView<BaseWorkspaceEditorView, DataGenerationWorkspaceEditorViewModel>(services)
            // Documents
            .AddView<TableDataView, TableDataViewModel>(services)
            .AddView<DataGenerationSchemeView, DataGenerationSchemaViewModel>(services)
            .AddView<MarkdownView, MarkdownViewModel>(services)
            // Tools
            .AddView<TableUtilitiesView, TableUtilitiesViewModel>(services)
            .AddView<TableSearchView, TableSearchViewModel>(services)
            .AddView<DataGenerationOptionsView, DataGenerationOptionsViewModel>(services)
            .AddView<TableColumnsEditorView, TableColumnsEditorViewModel>(services)
            // Misc
            .AddView<DataGenerationTypesSelectionListView, DataGenerationTypesSelectionListViewModel>(services)
            .AddView<DataGenerationTypesSelectionView, DataGenerationTypesSelectionViewModel>(services);
        
        return views;
    }

    private static ServiceProvider ConfigureServices(ServiceCollection services)
    {
        // Main Display Window
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();

        // Custom Services
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        services.AddSingleton<IFilesDialogManager, FilesDialogManager>();
        services.AddSingleton<IEventManager, EventManager>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        
        // Database Services
        services.AddDatabaseFactory<TableStoreDbContext, TableStoreDatabaseContextFactory>();
        
        // Register Command Handlers
        services.RegisterCommandHandlers();
        
        // Register File Converters
        services.RegisterFileConverters();

        return services.BuildServiceProvider();
    }

    private void OnException(IServiceProvider provider)
    {
        var toastManager = provider.GetRequiredService<ISukiToastManager>();
        
        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent($"An error occured during command execution:")
            .Queue();
    }
}