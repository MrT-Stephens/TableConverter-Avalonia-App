using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Linq;
using TableConverter.Common;
using TableConverter.Services;
using TableConverter.ViewModels;
using TableConverter.Views;

namespace TableConverter;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        ServiceProvider = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && ServiceProvider is not null)
        {
            var viewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var view = ServiceProvider.GetRequiredService<MainWindowView>();

            view.DataContext = viewModel;
            desktop.MainWindow = view;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // ViewLocator
        var viewLocator = Current?.DataTemplates.First(val => val is ViewLocator);

        if (viewLocator is not null)
        {
            services.AddSingleton(viewLocator);
        }

        // Views
        services.AddSingleton<MainWindowView>();

        // Custom Services
        services.AddSingleton<ConverterTypesService>();
        services.AddSingleton<DataGenerationTypesService>();

        // SukiUI Services
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<BasePageViewModel, WelcomePageViewModel>();
        services.AddSingleton<BasePageViewModel, ConvertFilesPageViewModel>();
        services.AddSingleton<BasePageViewModel, DataGenerationPageViewModel>();

        return services.BuildServiceProvider();
    }
}
