using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationDateTimeHandler : DataGenerationTypeHandlerAbstract<DataGenerationDateTimeOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationDateTimeOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() => GenerateRandomDateTime(
                        options!.FromDateTime, 
                        options!.ToDateTime
                    ).ToString(
                        options!.DateTimeFormats[options!.SelectedDateTimeFormat]
                    ), blanks_percentage);
            }

            return data;
        }

        public DateTime GenerateRandomDateTime(DateTime startDate, DateTime endDate)
        {
            // Calculate the range in ticks
            long range = endDate.Ticks - startDate.Ticks;

            // Generate a random offset within the range
            long ticksOffset = (long)(Random.NextDouble() * range);

            // Create a new DateTime by adding the offset to the start date
            return startDate.AddTicks(ticksOffset);
        }

    }
}
