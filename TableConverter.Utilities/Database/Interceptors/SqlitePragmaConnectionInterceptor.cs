using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TableConverter.Utilities.Database.Interceptors;

/// <summary>
/// Applies the SQLite pragmas that cannot be expressed through a connection string whenever EF
/// Core opens a physical connection. The pragmas that <em>can</em> be expressed through the
/// connection string (foreign keys, pooling) are configured by
/// <see cref="Factories.TableStoreDbContextFactory"/> instead.
/// </summary>
public sealed class SqlitePragmaConnectionInterceptor : DbConnectionInterceptor
{
    private const string Pragmas = """
        PRAGMA journal_mode = WAL;
        PRAGMA synchronous = NORMAL;
        PRAGMA temp_store = MEMORY;
        PRAGMA busy_timeout = 5000;
        """;

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ApplyPragmas(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ApplyPragmasAsync(connection, cancellationToken).ConfigureAwait(false);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken).ConfigureAwait(false);
    }

    private static void ApplyPragmas(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        command.ExecuteNonQuery();
    }

    private static async Task ApplyPragmasAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}

