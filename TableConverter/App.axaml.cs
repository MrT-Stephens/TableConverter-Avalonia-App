using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Common;
using TableConverter.Components.Xaml;
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
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);

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
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ConvertFilesManagerService>();
        services.AddSingleton<ConverterTypesService>();
        services.AddSingleton<DataGenerationTypesService>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        return services.BuildServiceProvider();
    }
}