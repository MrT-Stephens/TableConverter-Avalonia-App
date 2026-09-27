using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Configuration;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Utilities.Database.Contexts;

public sealed class TableStoreDbContext(DbContextOptions<TableStoreDbContext> options) 
    : DbContext(options)
{
    /// <remarks>
    /// This is the only public constructor on purpose: EF Core creates the context instances on
    /// behalf of <see cref="Factories.TableStoreDbContextFactory"/>, and the per data source state
    /// (path, source id and the services the context needs) travels in the options.
    /// </remarks>
    private readonly TableStoreOptionsExtension _tableStore = options
        .FindExtension<TableStoreOptionsExtension>()
        ?? throw new InvalidOperationException(
            "TableStoreDbContext requires options configured with UseTableStore(...). " +
            "Use ITableStoreDbContextFactory to create table store contexts.");

    /// <summary>
    /// The table store this context is bound to.
    /// </summary>
    public string Path => _tableStore.Path;

    public Guid SourceId => _tableStore.SourceId;
    public DbSet<ColumnEntity> Columns => Set<ColumnEntity>();
    public DbSet<RowEntity> Rows => Set<RowEntity>();
    public DbSet<CellEntity> Cells => Set<CellEntity>();
    public DbSet<SearchResult> SearchResults => Set<SearchResult>();

    /// <summary>
    ///     The edits made to this store, newest last.
    /// </summary>
    /// <remarks>
    ///     The history lives in the store it describes rather than beside it in memory, so how far a table
    ///     can be walked back survives the document being closed and does not have to be held for every
    ///     table that is open at once.
    /// </remarks>
    public DbSet<HistoryEntryEntity> History => Set<HistoryEntryEntity>();

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
                // Stored as its numeric value, which is the value the column type carries, so renaming an
                // enum member can never change what an existing store holds.
                .HasConversion<int>()
                .IsRequired();
            
            b.Property(x => x.OrdinalPosition)
                .HasColumnName("ORDINAL_POSITION")
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

        modelBuilder.Entity<HistoryEntryEntity>(b =>
        {
            b.ToTable("TABLE_HISTORY");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();

            b.Property(x => x.Sequence)
                .HasColumnName("SEQUENCE")
                .IsRequired();

            // Stored as a number of seconds since the Unix epoch: SQLite has no date type, and every
            // provider agrees on what an integer is.
            b.Property(x => x.Timestamp)
                .HasColumnName("TIMESTAMP")
                .IsRequired();

            b.Property(x => x.Description)
                .HasColumnName("DESCRIPTION")
                .IsRequired();

            b.Property(x => x.Kind)
                .HasColumnName("KIND")
                .IsRequired();

            b.Property(x => x.Payload)
                .HasColumnName("PAYLOAD")
                .IsRequired();

            b.Property(x => x.IsApplied)
                .HasColumnName("IS_APPLIED")
                .IsRequired();

            // The history is read in the order its steps were taken, and the steps that can be undone are
            // found by whether they are applied, so both are indexed rather than scanned for.
            b.HasIndex(x => x.Sequence).HasDatabaseName("IDX_TABLE_HISTORY_SEQUENCE");
            b.HasIndex(x => x.IsApplied).HasDatabaseName("IDX_TABLE_HISTORY_APPLIED");
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
                _tableStore.EventManager
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
                _tableStore.EventManager
                    .GetEvent<DbEntityChangedEvent>()
                    .Publish(new DbEntityChangedEventArgs
                    {
                        SourceId = SourceId,
                        Type = change.Key,
                        Changes = [.. change.Value]
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

            if (state == DbEntityChangeState.None)
                continue;

            var modifiedProperties = entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => (p.OriginalValue, p.CurrentValue)
                );

            var type = entry.Metadata.ClrType;

            if (!result.TryGetValue(type, out var list))
            {
                list = [];
                result[type] = list;
            }

            list.Add(new DbEntityChange(entry.Entity, state, modifiedProperties));
        }

        return result;
    }
}