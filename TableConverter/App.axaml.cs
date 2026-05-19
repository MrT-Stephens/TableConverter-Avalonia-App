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
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using ModelFlow.DataVirtualization;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Enums;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.Configuration;
using TableConverter.FileConverters.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Services.DataSources;
using TableConverter.Utilities;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Factories;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Logging;
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
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        VirtualizationManager.Instance.UiThreadExcecuteAction = a => 
            Dispatcher.UIThread.InvokeAsync(a).GetTask();
            
        DispatcherTimer.Run(() =>
        {
            VirtualizationManager.Instance.ProcessActions();
            return true;
        }, TimeSpan.FromMilliseconds(10), DispatcherPriority.Background);
            
        var services = new ServiceCollection();
            
        ConfigureViews(services);
            
        var provider = ConfigureServices(services);
            
        provider.RegisterCommandHandlers();
        provider.RegisterCommandError();
            
        DataSource.DataSourceCallbacks = provider.GetRequiredService<IDataSourceCallbacks>();

        DataTemplates.Add(provider.GetRequiredService<IDataTemplate>());

        var toastService = provider.GetRequiredService<ISukiToastManager>()
            ?? throw new InvalidOperationException("Failed to create toast manager");
        
        var dialogService = provider.GetRequiredService<ISukiDialogManager>()
            ?? throw new InvalidOperationException("Failed to create dialog manager");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = provider.GetRequiredService<MainWindowView>()
                ?? throw new InvalidOperationException("Failed to create main window");
            
            var viewModel = provider.GetRequiredService<MainWindowViewModel>()
                ?? throw new InvalidOperationException("Failed to create main window view model");
            
            window.DataContext = viewModel;

            window.Hosts.Add(new SukiToastHost
            {
                Manager = toastService,
            });

            window.Hosts.Add(new SukiDialogHost
            {
                Manager = dialogService,
            });

            desktop.MainWindow = window;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            // Browser setup: wrap the main content in a panel with background and dialog host
            var mainViewModel = provider.GetRequiredService<MainWindowViewModel>();
            
            var panel = new Panel();
            panel.Children.Add(new SukiBackground { Style = SukiBackgroundStyle.Bubble });
            
            var mainContentView = new MainContentView { DataContext = mainViewModel };
            panel.Children.Add(mainContentView);
            
            single.MainView = new SukiMainHost
            {
                Hosts = 
                [
                    new SukiDialogHost
                    {
                        Manager = dialogService,
                    },
                    new SukiToastHost
                    {
                        Manager = toastService,
                    }
                ],
                Content = panel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureViews(ServiceCollection services)
    {
        var views = new ViewsCollection();
        
        // Workspaces
        views.AddView<BaseWorkspaceEditorView, TableWorkspaceEditorViewModel>(services);
        views.AddView<BaseWorkspaceEditorView, DataGenerationWorkspaceEditorViewModel>(services);
        // Documents
        views.AddView<TableDataView, TableDataViewModel>(services);
        views.AddView<DataGenerationSchemeView, DataGenerationSchemaViewModel>(services);
        views.AddView<MarkdownView, MarkdownViewModel>(services);
        // Tools
        views.AddView<TableUtilitiesView, TableUtilitiesViewModel>(services);
        views.AddView<TableSearchView, TableSearchViewModel>(services);
        views.AddView<DataGenerationOptionsView, DataGenerationOptionsViewModel>(services);
        views.AddView<TableColumnsEditorView, TableColumnsEditorViewModel>(services);
        // Misc
        views.AddView<DataGenerationTypesSelectionListView, DataGenerationTypesSelectionListViewModel>(services);
        views.AddView<DataGenerationTypesSelectionView, DataGenerationTypesSelectionViewModel>(services);
        
        // Register Views Collection
        services.AddSingleton<IViewsCollection, ViewsCollection>(_ => views);
        
        // Register Data Templates
        services.AddSingleton<IDataTemplate, ViewLocator>();
    }

    private static ServiceProvider ConfigureServices(ServiceCollection services)
    {
        // Register Configuration
        var configBuilder = new ConfigurationBuilder();
        
        // In browser (WASM), appsettings.json may not be accessible via file system
        // Make it optional for WASM builds
        if (OperatingSystem.IsBrowser())
        {
            configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        }
        else
        {
            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }
        
        IConfiguration configuration = configBuilder.Build();
        
        services.AddSingleton(configuration);

        // Configure App Options
        services.AddOptions<AppOptions>().Bind(configuration.GetSection(nameof(AppOptions)));
        
        // Register Logging
        // In browser builds, skip file-based logging as there's no traditional file system
        if (!OperatingSystem.IsBrowser())
        {
            var baseDirectory = configuration.GetSection(nameof(AppOptions))
                .Get<AppOptions>()!.BaseContentPath;
            
            services.AddLogging(builder => builder.AddFile(configuration.GetSection("Logging"),
                options =>
                {
                    options.FormatLogFileName = name => Path.Combine(
                        baseDirectory, string.Format(name, DateTime.UtcNow));
                }));
        }
        else
        {
            // For browser builds, use minimal logging configuration
            services.AddLogging(builder => { });
        }

        services.AddSingleton<IDataSourceCallbacks, LoggingDataSourceCallbacks>();
        
        // Main Display Window
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();

        // Custom Services
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        // Use browser-friendly replacements when running in WASM
        if (OperatingSystem.IsBrowser())
        {
            services.AddSingleton<IFilesDialogManager, BrowserFilesDialogManager>();
        }
        else
        {
            services.AddSingleton<IFilesDialogManager, FilesDialogManager>();
        }

        services.AddSingleton<IEventManager, EventManager>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        
        // Database Services
        if (OperatingSystem.IsBrowser())
        {
            // Use in-memory EF provider in browser to avoid native SQLite dependency
            services.AddDatabaseFactory<TableStoreDbContext, TableConverter.Utilities.Database.Factories.BrowserTableStoreDatabaseContextFactory>();
        }
        else
        {
            services.AddDatabaseFactory<TableStoreDbContext, TableStoreDatabaseContextFactory>();
        }
        
        // Register Command Handlers
        services.RegisterCommandHandlers();
        
        // Register File Converters
        services.RegisterFileConverters();

        return services.BuildServiceProvider();
    }
}