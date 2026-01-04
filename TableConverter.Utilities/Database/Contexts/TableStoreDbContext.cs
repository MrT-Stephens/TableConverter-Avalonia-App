using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Models;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database.Contexts;

public sealed class TableStoreDbContext(DbContextOptions<TableStoreDbContext> options) : DbContext(options)
{
    public DbSet<ColumnEntity> Columns => Set<ColumnEntity>();
    public DbSet<RowEntity> Rows => Set<RowEntity>();
    public DbSet<CellEntity> Cells => Set<CellEntity>();
    public DbSet<SearchResult> SearchResults => Set<SearchResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColumnEntity>(builder =>
        {
            builder.ToTable("COLUMNS");

            builder.HasKey(x => x.ColumnId);

            builder.Property(x => x.ColumnId)
                .HasColumnName("COLUMN_ID")
                .ValueGeneratedOnAdd();
            
            builder.Property(x => x.Name)
                .HasColumnName("NAME")
                .IsRequired();
            
            builder.Property(x => x.DataType)
                .HasColumnName("DATA_TYPE")
                .IsRequired();
            
            builder.Property(x => x.Ordinal)
                .HasColumnName("ORDINAL")
                .IsRequired();
        });

        modelBuilder.Entity<RowEntity>(b =>
        {
            b.ToTable("ROWS");

            b.HasKey(x => x.RowId);

            b.Property(x => x.RowId)
                .HasColumnName("ROW_ID")
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<CellEntity>(b =>
        {
            b.ToTable("CELLS");

            // PRIMARY KEY (ROW_ID, COLUMN_ID)
            b.HasKey(x => new { x.RowId, x.ColumnId });

            // COLUMN DEFINITIONS
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
            b.HasKey(x => new { x.RowId, x.ColumnId });

            // COLUMN DEFINITIONS
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
}