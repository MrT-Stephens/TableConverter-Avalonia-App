using System.Data.SQLite;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationMovieGenresHandler : DataGenerationTypeHandlerAbstract
    {
        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                using (var reader = DbConnection.ExecuteCommand(
                    $"SELECT M.NAME, GROUP_CONCAT(G.GENRE, ', ') FROM MOVIES_TABLE M JOIN MOVIES_TO_GENRES_MAP_TABLE MG ON M.ID = MG.MOVIE_ID " + 
                    $"JOIN MOVIES_GENRES_TABLE G ON MG.GENRE = G.GENRE WHERE M.ID IN (SELECT ID FROM MOVIES_TABLE ORDER BY RANDOM() LIMIT {rows}) GROUP BY M.ID;"
                ))
                {
                    if (!reader.HasRows)
                    {
                        throw new SQLiteException("No rows returned from the database.");
                    }

                    long i = 0;

                    while (reader.Read())
                    {
                        data[i++] = CheckBlank(() => reader.GetString(1), blanks_percentage);
                    }

                    if (i < rows)
                    {
                        for (long j = i; j < rows; j++)
                        {
                            data[j] = data[j - i];
                        }
                    }
                };

                return data;
            });
        }
    }
}
