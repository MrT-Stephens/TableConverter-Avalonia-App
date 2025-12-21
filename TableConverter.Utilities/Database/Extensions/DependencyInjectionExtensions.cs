using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Utilities.Database.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConnectionFactory<TImplementation>(this IServiceCollection serviceCollection, string key)
        where TImplementation : class, IConnectionFactory
    {
        serviceCollection.AddKeyedSingleton<IConnectionFactory, TImplementation>(key);
        return serviceCollection;
    }
}