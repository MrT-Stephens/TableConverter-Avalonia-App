using System.Data.SQLite;
using System.Data;

namespace TableConverter.Services
{
    internal class DataGenerationDataBaseService
    {
        private readonly string ConnectionString = "Data Source=TableConverter-Data-Generation.db";

        private SQLiteConnection Connection { get; init; }

        public DataGenerationDataBaseService()
        {
            Connection = new SQLiteConnection(ConnectionString);
        }

        public SQLiteDataReader ExecuteCommand(string query)
        {
            Connection.Open();

            SQLiteCommand command = new SQLiteCommand(query, Connection);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
