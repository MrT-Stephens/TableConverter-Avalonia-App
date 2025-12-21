using Microsoft.Data.Sqlite;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Utilities.Database;

/// <summary>
/// Base class for SQLite connections. Handles directory creation,
/// PRAGMA configuration, and schema initialization.
/// </summary>
public abstract class ConnectionFactoryBase : IConnectionFactory
{
    #region Public Methods

    /// <summary>
    /// Opens a SQLite connection to the specified database file,
    /// applies PRAGMAs, and ensures the database schema exists.
    /// </summary>
    /// <param name="databasePath">The path to the SQLite database file.</param>
    /// <returns>An open <see cref="SqliteConnection"/>.</returns>
    public SqliteConnection Open(string databasePath)
    {
        EnsureDirectory(databasePath);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        ApplyPragmas(connection);
        EnsureSchema(connection);

        return connection;
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Returns the SQL schema code to initialize the database.
    /// </summary>
    /// <returns>SQL schema script.</returns>
    protected abstract string GetSchemaCode();

    #endregion

    #region Protected Methods

    /// <summary>
    /// Ensures the directory for the database exists.
    /// </summary>
    /// <param name="databasePath">The database file path.</param>
    protected virtual void EnsureDirectory(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Applies standard SQLite PRAGMA settings.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    protected virtual void ApplyPragmas(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        
        cmd.CommandText =
            """
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA foreign_keys = ON;
            """;

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Ensures the database schema exists by executing the SQL returned by <see cref="GetSchemaCode"/>.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    protected virtual void EnsureSchema(SqliteConnection connection)
    {
        var schemaSql = GetSchemaCode();

        if (string.IsNullOrWhiteSpace(schemaSql))
        {
            return;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = schemaSql;
        
        cmd.ExecuteNonQuery();
    }

    #endregion
}
