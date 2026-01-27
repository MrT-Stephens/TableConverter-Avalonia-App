using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Factories;

public sealed class TableStoreDatabaseContextFactory(IEventManager eventManager) 
    : DatabaseContextFactoryBase<TableStoreDbContext>(
        (options, events, sourceId) => new TableStoreDbContext(options, events, sourceId), 
        eventManager)
{
}