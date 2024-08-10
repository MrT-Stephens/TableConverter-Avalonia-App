using System.Data.SQLite;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationMovieGenresHandler : DataGenerationTypeHandlerAbstract<DataGenerationBaseOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
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
        }
    }
}
