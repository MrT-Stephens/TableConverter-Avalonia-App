using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database.Factories;

public sealed class TableStoreDbContextFactory() 
    : DbContextFactoryBase<TableStoreDbContext>(options => new TableStoreDbContext(options))
{
}