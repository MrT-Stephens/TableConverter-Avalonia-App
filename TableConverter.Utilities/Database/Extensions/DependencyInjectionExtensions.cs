using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Factories;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Interceptors;

namespace TableConverter.Utilities.Database.Extensions;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers <see cref="TableStoreDbContext"/> through EF Core's built-in
    /// <see cref="IDbContextFactory{TContext}"/> infrastructure.
    /// </summary>
    /// <remarks>
    /// The provider is chosen per data source by <see cref="TableStoreDbContextFactory"/>: SQLite
    /// on desktop and the in-memory provider in the browser.
    /// </remarks>
    public static IServiceCollection AddTableStoreDatabase(this IServiceCollection services)
    {
        services.AddDbContextFactory<TableStoreDbContext, TableStoreDbContextFactory>((_, options) =>
        {
            // SQLite only; ignored by the in-memory provider used in the browser.
            if (!OperatingSystem.IsBrowser())
            {
                options.AddInterceptors(new SqlitePragmaConnectionInterceptor());
            }
        }, ServiceLifetime.Singleton);

        // Expose the path aware factory (registered above as EF's IDbContextFactory) so that
        // callers can target a specific table store file.
        services.AddSingleton<ITableStoreDbContextFactory>(
            sp => (TableStoreDbContextFactory)sp.GetRequiredService<IDbContextFactory<TableStoreDbContext>>());

        return services;
    }
}