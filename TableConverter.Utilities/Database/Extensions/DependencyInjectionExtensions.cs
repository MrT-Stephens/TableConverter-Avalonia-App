using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TableConverter.Utilities.Database.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDatabaseFactory<TDbContext, TFactory>(this IServiceCollection services)
        where TDbContext : DbContext
        where TFactory : class, Interfaces.IDatabaseContextFactory<TDbContext>
    {
        services.AddSingleton<Interfaces.IDatabaseContextFactory<TDbContext>, TFactory>();
        return services;
    }
}