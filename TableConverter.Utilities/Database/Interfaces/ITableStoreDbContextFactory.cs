using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database.Interfaces;

/// <summary>
/// Path aware factory for <see cref="TableStoreDbContext"/>.
/// </summary>
/// <remarks>
/// Extends EF Core's built-in <see cref="IDbContextFactory{TContext}"/> with overloads that
/// target a specific table store data source (one SQLite file, or one in-memory store in the
/// browser). Callers should use the path based overloads; the parameterless members inherited
/// from <see cref="IDbContextFactory{TContext}"/> are not supported because a table store
/// cannot be opened without a path.
/// </remarks>
public interface ITableStoreDbContextFactory : IDbContextFactory<TableStoreDbContext>
{
    /// <summary>
    /// Creates a context for the table store at <paramref name="path"/>, creating the
    /// underlying database if it does not exist yet.
    /// </summary>
    TableStoreDbContext CreateDbContext(string path);

    /// <summary>
    /// Creates a context for the table store at <paramref name="path"/>, creating the
    /// underlying database if it does not exist yet.
    /// </summary>
    Task<TableStoreDbContext> CreateDbContextAsync(string path, CancellationToken cancellationToken = default);
}

