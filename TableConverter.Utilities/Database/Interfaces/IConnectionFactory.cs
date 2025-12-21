using Microsoft.Data.Sqlite;

namespace TableConverter.Utilities.Database.Interfaces;

public interface IConnectionFactory
{
    public SqliteConnection Open(string databasePath);
}