namespace TableConverter.Utilities.Database;

public class TableDataConnectionFactory : ConnectionFactoryBase
{
    protected override string GetSchemaCode()
    {
        return """
               CREATE TABLE IF NOT EXISTS COLUMNS (
                   COLUMN_ID INTEGER PRIMARY KEY,
                   NAME TEXT NOT NULL,
                   DATA_TYPE INTEGER NOT NULL,
                   ORDINAL INTEGER NOT NULL
               );

               CREATE TABLE IF NOT EXISTS ROWS (
                   ROW_ID INTEGER PRIMARY KEY
               );

               CREATE TABLE IF NOT EXISTS CELLS (
                   ROW_ID INTEGER NOT NULL,
                   COLUMN_ID INTEGER NOT NULL,
                   VALUE TEXT,
                   PRIMARY KEY (ROW_ID, COLUMN_ID),
                   FOREIGN KEY (ROW_ID) REFERENCES ROWS(ROW_ID) ON DELETE CASCADE,
                   FOREIGN KEY (COLUMN_ID) REFERENCES COLUMNS(COLUMN_ID) ON DELETE CASCADE
               );

               CREATE INDEX IF NOT EXISTS IDX_CELLS_ROW ON CELLS(ROW_ID);
               CREATE INDEX IF NOT EXISTS IDX_CELLS_COL ON CELLS(COLUMN_ID);
               """;
    }
}