using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Factories;

public sealed class TableStoreDatabaseContextFactory(IEventManager eventManager, ILoggerFactory loggerFactory) 
    : DatabaseContextFactoryBase<TableStoreDbContext>(
        (options, events, sourceId) => new TableStoreDbContext(options, events, sourceId), 
        eventManager, loggerFactory)
{
}