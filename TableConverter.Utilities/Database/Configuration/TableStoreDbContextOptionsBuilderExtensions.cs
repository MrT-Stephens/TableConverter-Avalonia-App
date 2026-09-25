using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Configuration;

public static class TableStoreDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Binds the options to the table store at <paramref name="path"/>.
    /// </summary>
    public static DbContextOptionsBuilder UseTableStore(
        this DbContextOptionsBuilder optionsBuilder,
        string path,
        IEventManager eventManager)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new TableStoreOptionsExtension(path, eventManager));

        return optionsBuilder;
    }
}

