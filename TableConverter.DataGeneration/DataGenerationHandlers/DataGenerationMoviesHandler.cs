using System.Data.SQLite;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationMoviesHandler : DataGenerationTypeHandlerAbstract<DataGenerationMoviesOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationMoviesOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            using (var reader = DbConnection.ExecuteCommand(
                $"SELECT M.NAME FROM MOVIES_TABLE M INNER JOIN MOVIES_TO_GENRES_MAP_TABLE MG ON M.ID = MG.MOVIE_ID INNER JOIN MOVIES_GENRES_TABLE G ON MG.GENRE = G.GENRE WHERE G.GENRE = '{options!.MovieGenre}'" + 
                $"AND M.ID IN (SELECT ID FROM MOVIES_TABLE WHERE ID IN (SELECT MG.MOVIE_ID FROM MOVIES_TO_GENRES_MAP_TABLE MG JOIN MOVIES_GENRES_TABLE G ON MG.GENRE = G.GENRE WHERE G.GENRE = '{options!.MovieGenre}') ORDER BY RANDOM() LIMIT {rows});"
            ))
            {
                if (!reader.HasRows)
                {
                    throw new SQLiteException("No rows returned from the database.");
                }

                long i = 0;

                while (reader.Read())
                {
                    data[i++] = CheckBlank(() => reader.GetString(0), blanks_percentage);
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
