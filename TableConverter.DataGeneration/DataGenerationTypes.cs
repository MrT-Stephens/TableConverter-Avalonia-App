using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration
{
    public class DataGenerationGenHandler
    {
        /// <summary>
        /// Generates data based on the data generation fields and the number of rows.
        /// </summary>
        /// <param name="dataGenerationFields"> The <seealso cref="DataGenerationField"/> that holds data about the generator, column name, blanks percentage. </param>
        /// <param name="numberOfRows"> The number of rows to generate. </param>
        /// <returns> A <seealso cref="TableData"/> object which holds the tabular data. </returns>
        public TableData GenerateData(DataGenerationField[] dataGenerationFields, int numberOfRows)
        {
            List<string> headers = new List<string>();
            List<string[]> rows = new List<string[]>();

            for (int i = 0; i < numberOfRows; i++)
            {
                rows.Add(new string[dataGenerationFields.LongLength]);
            }

            for (int i = 0; i < dataGenerationFields.LongLength; i++)
            {
                headers.Add((!string.IsNullOrEmpty(dataGenerationFields[i].Name)) ? dataGenerationFields[i].Name : dataGenerationFields[i].Type);

                string[] column = dataGenerationFields[i].TypeHandler.GenerateData(numberOfRows, dataGenerationFields[i].BlankPercentage);

                for (int j = 0; j < column.LongLength; j++)
                {
                    rows[j][i] = column[j];
                }
            }

            return new TableData(headers, rows);
        }

        /// <summary>
        /// Generates data based on the data generation fields and the number of rows asynchronously.
        /// </summary>
        /// <param name="dataGenerationFields"> The <seealso cref="DataGenerationField"/> that holds data about the generator, column name, blanks percentage. </param>
        /// <param name="numberOfRows"> The number of rows to generate. </param>
        /// <returns> A <seealso cref="Task{TableData}"/> object which holds the tabular data. Needs to be awaited. </returns>
        public async Task<TableData> GenerateDataAsync(DataGenerationField[] dataGenerationFields, int numberOfRows)
        {
            List<string> headers = new List<string>();
            List<string[]> rows = new List<string[]>();

            for (int i = 0; i < numberOfRows; i++)
            {
                rows.Add(new string[dataGenerationFields.LongLength]);
            }

            for (int i = 0; i < dataGenerationFields.LongLength; i++)
            {
                headers.Add(!string.IsNullOrEmpty(dataGenerationFields[i].Name) ? dataGenerationFields[i].Name : dataGenerationFields[i].Type);
            }

            var tasks = dataGenerationFields.Select(async (field, index) =>
            {
                string[] column = await field.TypeHandler.GenerateDataAsync(numberOfRows, field.BlankPercentage);

                for (int j = 0; j < column.LongLength; j++)
                {
                    rows[j][index] = column[j];
                }
            });

            await Task.WhenAll(tasks);

            return new TableData(headers, rows);
        }
    }
}
