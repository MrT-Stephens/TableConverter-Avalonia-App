using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.Extensions;

public static class DependencyInjectionExtensions
{
    public static void RegisterCommandHandlers(this IServiceCollection services, Assembly assembly)
    {
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
}