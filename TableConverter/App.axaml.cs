using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NPOI.POIFS.Crypt.Dsig;
using SukiUI.Dialogs;
using SukiUI.Theme.Shadcn;
using SukiUI.Toasts;
using TableConverter.Commands;
using TableConverter.Commands.Interfaces;
using TableConverter.Commands.Services;
using TableConverter.Common;
using TableConverter.Components.Xaml;
using TableConverter.DataModels;
using TableConverter.Interfaces;
using TableConverter.Services;
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
            .AddView<MainWindowView, MainWindowViewModel>(services)
            // Add Views
            .AddView<WelcomePageView, WelcomePageViewModel>(services)
            .AddView<ConvertFilesPageView, ConvertFilesPageViewModel>(services)
            .AddView<DataGenerationPageView, DataGenerationPageViewModel>(services)
            .AddView<DataGenerationListTypesView, DataGenerationListTypesViewModel>(services)
            // Add Dialogs Views
            .AddView<FileTypesSelectorView, FileTypesSelectorViewModel>(services)
            .AddView<ConvertFilesOptionsView, ConvertFilesOptionsViewModel>(services)
            .AddView<DataGenerationTypesView, DataGenerationTypesViewModel>(services);

        return views;
    }

    private static IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // Custom Services
        services.AddSingleton<IPageNavigation, PageNavigation>();
        services.AddSingleton<IConverterTypes, ConverterTypes>();
        services.AddSingleton<IDataGenerationTypes, DataGenerationTypes>();
        services.AddSingleton<ConvertFilesManager>();
        services.AddSingleton<IFilesDialogManager, FilesDialogManager>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        
        // Command Manager
        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<ICommandHandlerAsync, AddFileCommandHandler>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureCommands(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<ICommandManager>();
        
        foreach (var handler in provider.GetServices<ICommandHandlerAsync>())
        {
            manager.RegisterCommandAsync("AddFile", handler);
        }
    }
}