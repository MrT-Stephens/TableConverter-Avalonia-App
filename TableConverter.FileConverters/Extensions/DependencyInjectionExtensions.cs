using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterFileConverters(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        
        foreach (var type in assembly.GetTypes().Where(t => 
            typeof(IConverterProvider).IsAssignableFrom(t) 
            && !t.IsAbstract))
        {
            services.AddSingleton(typeof(IConverterProvider), type);
        }
        
        return services;
    }
}