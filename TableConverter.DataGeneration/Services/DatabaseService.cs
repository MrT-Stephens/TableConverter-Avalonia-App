using System.Data.SQLite;

namespace TableConverter.DataGeneration.Services
{
    public class DatabaseService
    {
        private SQLiteConnection Connection { get; init; }

        public DatabaseService(string connectionString)
        {
            Connection = new SQLiteConnection(connectionString);
        }

        public SQLiteDataReader ExecuteCommand(string query)
        {
            Connection.Open();

            SQLiteCommand command = new SQLiteCommand(query, Connection);

            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}
