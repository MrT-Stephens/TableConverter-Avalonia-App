using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace TableConverter.Utilities.Database;

/// <summary>
///     Runs a piece of work in a transaction, joining the one that is already open rather than opening a
///     second.
/// </summary>
/// <remarks>
///     <para>
///         An operation that runs more than one statement has to succeed or fail as a whole, so it runs in
///         a transaction. Such an operation can be run on its own, when it opens the transaction itself, or
///         as one part of something larger - a history step being replayed, say - when the transaction
///         belongs to whoever asked for the operation.
///     </para>
///     <para>
///         A connection holds one transaction at a time, so an operation that opened one regardless would
///         fail every time it was run from inside another. Whoever opens the transaction is the one that
///         commits it, so an operation that finds one already open leaves the committing and the rolling
///         back to them.
///     </para>
/// </remarks>
internal static class DatabaseTransaction
{
    /// <summary>
    ///     Runs <paramref name="work" /> in a transaction, and reports what it produced.
    /// </summary>
    public static async Task<T> RunAsync<T>(
        this DatabaseFacade database,
        Func<Task<T>> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(work);

        var owner = database.CurrentTransaction is null;

        var transaction = owner
            ? await database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;

        try
        {
            var result = await work().ConfigureAwait(false);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    ///     Runs <paramref name="work" /> in a transaction.
    /// </summary>
    public static Task RunAsync(
        this DatabaseFacade database,
        Func<Task> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        return database.RunAsync<object?>(
            async () =>
            {
                await work().ConfigureAwait(false);
                return null;
            },
            cancellationToken);
    }
}

