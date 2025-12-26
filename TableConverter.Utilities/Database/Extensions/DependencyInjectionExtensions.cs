using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TableConverter.Utilities.Database.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDatabaseFactory<TDbContext, TFactory>(this IServiceCollection services)
        where TDbContext : DbContext
        where TFactory : class, Interfaces.IDbContextFactory<TDbContext>
    {
        services.AddSingleton<Interfaces.IDbContextFactory<TDbContext>, TFactory>();
        return services;
    }
}