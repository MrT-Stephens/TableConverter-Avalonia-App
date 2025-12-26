using Microsoft.EntityFrameworkCore;

namespace TableConverter.Utilities.Database.Interfaces;

public interface IDbContextFactory<TDbContext> where TDbContext : DbContext
{
    public TDbContext Create(string path);
    
    public Task<TDbContext> CreateAsync(string path, CancellationToken cancellationToken = default);
}