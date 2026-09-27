using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     Makes sure a store has a history table, including one that was created before the history existed.
/// </summary>
/// <remarks>
///     <para>
///         A store's schema is created by <c>EnsureCreated</c>, which only ever creates a database and never
///         changes one that is already there. A store that was written before the history existed therefore
///         has every table but this one, and asking it for a history would fail rather than come back empty.
///     </para>
///     <para>
///         Adding the table if it is missing is what a migration would do here, and it is written by hand
///         because the rest of the store's schema is: there are no migrations in this project to add one to,
///         and a store is a user's file rather than a database a deployment can be scheduled to change.
///     </para>
/// </remarks>
internal static class TableHistorySchema
{
    /// <summary>
    ///     The names of the table's columns, which have to match what the context maps the entity to.
    /// </summary>
    private const string CreateSql = """
        CREATE TABLE IF NOT EXISTS TABLE_HISTORY (
            ID INTEGER NOT NULL CONSTRAINT PK_TABLE_HISTORY PRIMARY KEY AUTOINCREMENT,
            SEQUENCE INTEGER NOT NULL,
            TIMESTAMP INTEGER NOT NULL,
            DESCRIPTION TEXT NOT NULL,
            KIND TEXT NOT NULL,
            PAYLOAD TEXT NOT NULL,
            IS_APPLIED INTEGER NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IDX_TABLE_HISTORY_SEQUENCE ON TABLE_HISTORY (SEQUENCE);
        CREATE INDEX IF NOT EXISTS IDX_TABLE_HISTORY_APPLIED ON TABLE_HISTORY (IS_APPLIED);
        """;

    /// <summary>
    ///     Adds the history table to a store that does not have one.
    /// </summary>
    public static void EnsureCreated(TableStoreDbContext db)
    {
        // There is no schema to add to when the store lives in memory, and nothing that has to be brought
        // up to date, because a store that only exists for the lifetime of the process was created by this
        // version of the context in the first place.
        if (!db.Database.IsRelational())
        {
            return;
        }

        db.Database.ExecuteSqlRaw(CreateSql);
    }

    /// <summary>
    ///     Adds the history table to a store that does not have one.
    /// </summary>
    public static async Task EnsureCreatedAsync(TableStoreDbContext db, CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync(CreateSql, cancellationToken).ConfigureAwait(false);
    }
}

