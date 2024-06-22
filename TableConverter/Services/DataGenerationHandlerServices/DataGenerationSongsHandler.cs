﻿using System.Data.SQLite;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationSongsHandler : DataGenerationTypeHandlerAbstract
    {
        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                using (var reader = DbConnection.ExecuteCommand(
                    $"SELECT NAME FROM SONGS_TABLE WHERE ID IN (SELECT ID FROM SONGS_TABLE ORDER BY RANDOM() LIMIT {rows});"
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
            });
        }
    }
}