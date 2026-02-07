using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.Extensions;

public static class DependencyInjectionExtensions
{
    public static void RegisterCommandHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICommandHandlerBase).IsAssignableFrom(t));

        handlerTypes.ForEach(type =>
        {
            services.AddSingleton(typeof(ICommandHandlerBase), type);
        });
    }
    
    public static void RegisterCommandHandlers(this IServiceProvider provider)
    {
        var commandService = provider.GetRequiredService<ICommandManager>();
        var handlers = provider.GetServices<ICommandHandlerBase>();
        
        handlers.ForEach(handler =>
        {
            commandService.RegisterCommand(handler.CommandMetadata.Name, handler);
        });
    }

    public static void RegisterCommandError(this IServiceProvider provider)
    {
        var commandService = provider.GetRequiredService<ICommandManager>();
        var toastManager = provider.GetRequiredService<ISukiToastManager>();

        commandService.OnError += (_, args) =>
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Error")
                .WithContent($"An error occured during command execution: {args.Message}")
                .Queue();
        };
    }
}