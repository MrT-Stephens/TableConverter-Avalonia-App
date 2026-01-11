using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Factories;

public sealed class TableStoreDbContextFactory(IEventManager eventManager) 
    : DbContextFactoryBase<TableStoreDbContext>(
        (options, events) => new TableStoreDbContext(options, events), 
        eventManager)
{
}