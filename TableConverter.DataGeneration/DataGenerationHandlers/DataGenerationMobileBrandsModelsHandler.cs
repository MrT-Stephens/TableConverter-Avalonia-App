using System.Data.SQLite;
using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationMobileBrandsModelsHandler : DataGenerationTypeHandlerAbstract<DataGenerationMobileBrandsModelsOptions>
    {
        protected override string[] GenerateDataOverride(int rows, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            using (var reader = DbConnection.ExecuteCommand(
                @$"SELECT MM.NAME FROM MOBILE_BRANDS_MODELS_TABLE MM 
                   INNER JOIN MOBILE_BRANDS_TABLE MB ON MB.ID = MM.BRAND_ID 
                   WHERE MB.NAME = '{Options!.MobileBrand}' AND MM.ID IN (
                      SELECT ID FROM MOBILE_BRANDS_MODELS_TABLE WHERE BRAND_ID = MB.ID ORDER BY RANDOM() LIMIT {rows}
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
