using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.Models;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Contexts;

public sealed class TableStoreDbContext(DbContextOptions<TableStoreDbContext> options, IEventManager eventsManager, Guid sourceId) 
    : DbContext(options)
{
    public Guid SourceId { get; } = sourceId;
    public DbSet<ColumnEntity> Columns => Set<ColumnEntity>();
    public DbSet<RowEntity> Rows => Set<RowEntity>();
    public DbSet<CellEntity> Cells => Set<CellEntity>();
    public DbSet<SearchResult> SearchResults => Set<SearchResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColumnEntity>(b =>
        {
            b.ToTable("COLUMNS");
            
            // PRIMARY KEY (COLUMN_ID)
            b.HasKey(x => x.Id);
            
            // COLUMN DEFINITIONS
            b.Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            
            b.Property(x => x.Name)
                .HasColumnName("NAME")
                .IsRequired();
            
            b.Property(x => x.DataType)
                .HasColumnName("DATA_TYPE")
                .IsRequired();

            b.Property(x => x.DefaultValueForCell)
                .HasColumnName("DEFAULT_VALUE_FOR_CELL");
        });

        modelBuilder.Entity<RowEntity>(b =>
        {
            b.ToTable("ROWS");

            // PRIMARY KEY (ROW_ID)
            b.HasKey(x => x.Id);

            // COLUMN DEFINITIONS
            b.Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            
            // FOREIGN KEY (ID) REFERENCES CELLS(ROW_ID) ON DELETE CASCADE
            b.HasMany(x => x.Cells)
                .WithOne(c => c.Row)
                .HasForeignKey(c => c.RowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CellEntity>(b =>
        {
            b.ToTable("CELLS");

            // PRIMARY KEY (ROW_ID, COLUMN_ID)
            b.HasKey(x => x.Id);

            // COLUMN DEFINITIONS
            b.Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            
            b.Property(x => x.RowId)
                .HasColumnName("ROW_ID")
                .IsRequired();
            
            b.Property(x => x.ColumnId)
                .HasColumnName("COLUMN_ID")
                .IsRequired();

            b.Property(x => x.Value)
                .HasColumnName("VALUE");

            // FOREIGN KEY (ROW_ID) REFERENCES ROWS(ROW_ID) ON DELETE CASCADE
            b.HasOne(x => x.Row)
                .WithMany(r => r.Cells)
                .HasForeignKey(x => x.RowId)
                .OnDelete(DeleteBehavior.Cascade);

            // FOREIGN KEY (COLUMN_ID) REFERENCES COLUMNS(COLUMN_ID) ON DELETE CASCADE
            b.HasOne(x => x.Column)
                .WithMany(c => c.Cells)
                .HasForeignKey(x => x.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            // CREATE INDEX IF NOT EXISTS IDX_CELLS_ROW ON CELLS(ROW_ID);
            b.HasIndex(x => x.RowId).HasDatabaseName("IDX_CELLS_ROW");

            // CREATE INDEX IF NOT EXISTS IDX_CELLS_COL ON CELLS(COLUMN_ID);
            b.HasIndex(x => x.ColumnId).HasDatabaseName("IDX_CELLS_COL");
            
            // CREATE INDEX IF NOT EXISTS IDX_CELLS_VALUE ON CELLS(VALUE);
            b.HasIndex(x => x.Value).HasDatabaseName("IDX_CELLS_VALUE");
        });

        modelBuilder.Entity<SearchResult>(b =>
        {
            b.ToTable("SEARCH_RESULT");

            // PRIMARY KEY (ROW_ID, COLUMN_ID)
            b.HasKey(x => x.Id);

            // COLUMN DEFINITIONS
            b.Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            
            b.Property(x => x.RowId)
                .HasColumnName("ROW_ID")
                .IsRequired();

            b.Property(x => x.ColumnId)
                .HasColumnName("COLUMN_ID")
                .IsRequired();

            b.Property(x => x.Value)
                .HasColumnName("VALUE")
                .IsRequired();

            b.Property(x => x.FoundValue)
                .HasColumnName("FOUND_VALUE")
                .IsRequired();
        });
    }

    public override int SaveChanges()
    {
        var changes = CollectEntityTypeChanges();
        
        var result = base.SaveChanges();
        
        if (changes.Count > 0)
        {
            changes.ForEach(change =>
            {
                eventsManager
                    .GetEvent<DbEntityChangedEvent>()
                    .Publish(new DbEntityChangedEventArgs
                    {
                        SourceId = SourceId,
                        Type = change.Key,
                        Changes = change.Value.ToArray()
                    });
            });
        }
        
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var changes = CollectEntityTypeChanges();
        
        var result = await base.SaveChangesAsync(cancellationToken);

        if (changes.Count > 0)
        {
            foreach (var change in  changes)
            {
                eventsManager
                    .GetEvent<DbEntityChangedEvent>()
                    .Publish(new DbEntityChangedEventArgs
                    {
                        SourceId = SourceId,
                        Type = change.Key,
                        Changes = change.Value.ToArray()
                    });
            }
        }
        
        return result;
    }
    
    private Dictionary<Type, List<DbEntityChange>> CollectEntityTypeChanges()
    {
        var result = new Dictionary<Type, List<DbEntityChange>>();

        foreach (var entry in ChangeTracker.Entries())
        {
            var state = entry.State switch
            {
                EntityState.Added    => DbEntityChangeState.Added,
                EntityState.Modified => DbEntityChangeState.Modified,
                EntityState.Deleted  => DbEntityChangeState.Deleted,
                _ => DbEntityChangeState.None
            };

            if (state is DbEntityChangeState.None)
            {
                continue;
            }

            var type = entry.Metadata.ClrType;

            if (result.TryGetValue(type, out var list))
            {
                list.Add(new DbEntityChange(entry.Entity, state));
            }
            else
            {
                result[type] = [new DbEntityChange(entry.Entity, state)];
            }
        }

        return result;
    }
}