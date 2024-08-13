using System.Data.SQLite;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationCompanyNameHandler : DataGenerationTypeHandlerAbstract<DataGenerationCompanyNameOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            using (var reader = DbConnection.ExecuteCommand(
                @$"SELECT CC.COUNTRY_CODE, CP.NAME FROM COMPANIES_TABLE CP 
                   INNER JOIN COUNTRY_CODES_TABLE CC ON CC.COUNTRY_CODE = '{Options!.CountryCode ?? "GB"}' 
                   WHERE CP.ID IN (
                      SELECT ID FROM COMPANIES_TABLE 
                      ORDER BY RANDOM() LIMIT {rows}
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
