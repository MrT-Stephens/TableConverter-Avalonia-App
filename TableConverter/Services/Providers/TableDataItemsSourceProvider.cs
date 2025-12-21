using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TableConverter.Utilities.Virtualisation.Interfaces;

namespace TableConverter.Services.Providers;

public class TableDataItemsSourceProvider(SqliteConnection connection) : IItemsProvider<string[]>
{
    private int[]? _columnIds;

    private int[] ColumnIds => _columnIds ??= FetchColumnIds();

    private int[] FetchColumnIds()
    {
        var columnIds = new List<int>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COLUMN_ID FROM COLUMNS ORDER BY ORDINAL";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            columnIds.Add(reader.GetInt32(0));
        }
        return columnIds.ToArray();
    }

    public int FetchCount()
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ROWS";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public IList<string[]> FetchRange(int startIndex, int pageCount, out int overallCount)
    {
        overallCount = FetchCount();
        var columnIds = ColumnIds;
        var items = new List<string[]>();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT ROW_ID FROM ROWS LIMIT @limit OFFSET @offset";
        cmd.Parameters.AddWithValue("@limit", pageCount);
        cmd.Parameters.AddWithValue("@offset", startIndex);

        var rowIds = new List<long>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                rowIds.Add(reader.GetInt64(0));
            }
        }

        foreach (var rowId in rowIds)
        {
            var row = new string[columnIds.Length];
            for (var i = 0; i < columnIds.Length; i++)
            {
                using var cellCmd = connection.CreateCommand();
                cellCmd.CommandText = "SELECT VALUE FROM CELLS WHERE ROW_ID = @rowId AND COLUMN_ID = @columnId";
                cellCmd.Parameters.AddWithValue("@rowId", rowId);
                cellCmd.Parameters.AddWithValue("@columnId", columnIds[i]);
                var value = cellCmd.ExecuteScalar();
                row[i] = value?.ToString() ?? string.Empty;
            }
            items.Add(row);
        }

        return items;
    }

    public void UpdateItem(int index, string[] item)
    {
        var rowId = GetRowIdAt(index);
        var columnIds = ColumnIds;

        for (var i = 0; i < Math.Min(columnIds.Length, item.Length); i++)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                              INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE)
                              VALUES (@rowId, @columnId, @value)
                              ON CONFLICT(ROW_ID, COLUMN_ID) DO UPDATE SET VALUE = @value
                              """;
            cmd.Parameters.AddWithValue("@rowId", rowId);
            cmd.Parameters.AddWithValue("@columnId", columnIds[i]);
            cmd.Parameters.AddWithValue("@value", item[i] ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public void InsertItem(int index, string[] item)
    {
        // SQLite doesn't have a built-in "insert at index" for rows unless we manage ORDINAL.
        // The ROWS table only has ROW_ID. 
        // If the collection is ordered by ROW_ID, we might need to shift things, but usually ROW_ID is autoincrement.
        // Assuming ROW_ID defines the order for now, which is not ideal for "insert at index".
        // However, looking at the schema, there is no ORDINAL in ROWS.
        
        using var transaction = connection.BeginTransaction();
        try
        {
            // Create a new row
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO ROWS DEFAULT VALUES; SELECT last_insert_rowid();";
            cmd.Transaction = transaction;
            var rowId = (long)cmd.ExecuteScalar()!;

            var columnIds = ColumnIds;
            for (var i = 0; i < Math.Min(columnIds.Length, item.Length); i++)
            {
                using var cellCmd = connection.CreateCommand();
                cellCmd.CommandText = "INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE) VALUES (@rowId, @columnId, @value)";
                cellCmd.Transaction = transaction;
                cellCmd.Parameters.AddWithValue("@rowId", rowId);
                cellCmd.Parameters.AddWithValue("@columnId", columnIds[i]);
                cellCmd.Parameters.AddWithValue("@value", item[i] ?? (object)DBNull.Value);
                cellCmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void RemoveItem(int index)
    {
        var rowId = GetRowIdAt(index);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM ROWS WHERE ROW_ID = @rowId";
        cmd.Parameters.AddWithValue("@rowId", rowId);
        cmd.ExecuteNonQuery();
    }

    public void ClearItems()
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM ROWS";
        cmd.ExecuteNonQuery();
    }

    private long GetRowIdAt(int index)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT ROW_ID FROM ROWS LIMIT 1 OFFSET @offset";
        cmd.Parameters.AddWithValue("@offset", index);
        var result = cmd.ExecuteScalar();
        if (result == null) throw new IndexOutOfRangeException();
        return (long)result;
    }
}