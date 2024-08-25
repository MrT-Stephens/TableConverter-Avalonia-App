using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using TableConverter.Services;
using TableConverter.ViewModels;

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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

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
