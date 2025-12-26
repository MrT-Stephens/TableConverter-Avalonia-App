using Microsoft.EntityFrameworkCore;

namespace TableConverter.Utilities.Database.Interfaces;

public interface IDbSession<out TDbContext> : IAsyncDisposable where TDbContext : DbContext
{
    public TDbContext DbContext { get; }
}