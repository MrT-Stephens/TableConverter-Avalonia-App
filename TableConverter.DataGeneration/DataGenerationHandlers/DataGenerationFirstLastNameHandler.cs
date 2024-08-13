using System.Data.SQLite;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationFirstLastNameHandler : DataGenerationTypeHandlerAbstract<DataGenerationFirstLastNameOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            using (var reader = DbConnection.ExecuteCommand(
                @$"SELECT C.COUNTRY_CODE, N.FIRST_NAME, N.LAST_NAME FROM FIRST_LAST_NAMES_TABLE N 
                   INNER JOIN COUNTRY_CODES_TABLE C ON C.COUNTRY_CODE = '{Options!.CountryCode ?? "GB"}' 
                   WHERE N.ID IN (
                      SELECT ID FROM FIRST_LAST_NAMES_TABLE ORDER BY RANDOM() LIMIT {rows}
                   );"
            ))
            {
                if (!reader.HasRows)
                {
                    throw new SQLiteException("No rows returned from the database.");
                }

                long i = 0;

                while (reader.Read())
                {
                    data[i++] = CheckBlank(() => $"{reader.GetString(1)} {reader.GetString(2)}", blanks_percentage);
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
