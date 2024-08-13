using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.Services;

namespace TableConverter.DataGeneration.DataModels
{
    public abstract class DataGenerationTypeHandlerAbstract<T> : IDataGenerationTypeHandler where T : DataGenerationBaseOptions, new()
    {
        /// <summary>
        /// The options for the generator. Used internally by <seealso cref="DataGenerationTypeHandlerAbstract{T}"/>, but can be used externally.
        /// </summary>
        dynamic? IDataGenerationTypeHandler.Options
        {
            get => this.Options;
            set => this.Options = value;
        }

        public T? Options { get; set; }

        /// <summary>
        /// A random data object for every instance of this class. Each instance has its own object.
        /// </summary>
        public Random Random { get; set; }

        /// <summary>
        /// Connection to the internal database for random data.
        /// </summary>
        public static DatabaseService DbConnection { get; } = new DatabaseService("");

        public DataGenerationTypeHandlerAbstract()
        {
            Options = (typeof(T) == typeof(DataGenerationBaseOptions)) ? null : new T();

            Random = new Random((int)DateTime.Now.ToBinary());
        }

        /// <summary>
        /// The generate data function that can be called by anyone.
        /// Will generate data based on the number of rows, options, and blanks percentage.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generator will generate. </param>
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        public string[] GenerateData(int rows, ushort blanks_percentage)
        {
            return GenerateDataOverride(rows, blanks_percentage);
        }

        /// <summary>
        /// The generate data function that can be called by anyone.
        /// Will generate data based on the number of rows, options, and blanks percentage asynchronously.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generator will generate. </param>s
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        public async Task<string[]> GenerateDataAsync(int rows, ushort blanks_percentage)
        {
            return await Task.Run(() => GenerateDataOverride(rows, blanks_percentage));
        }

        /// <summary>
        /// The internal generate function which gets called by <seealso cref="GenerateData(int, ushort)"/>.
        /// This function will get implemented by the extended versions of <seealso cref="DataGenerationTypeHandlerAbstract{T}"/>.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generator will generate. </param>
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        protected abstract string[] GenerateDataOverride(int rows, ushort blanks_percentage);


        /// <summary>
        /// The check blanks function. Will see if it needs to generate a blank value or not.
        /// </summary>
        /// <typeparam name="Func"> The type of the function. Must be a delegate type. </typeparam>
        /// <param name="func"> The function that will get called, if the value is not a blank value. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generator will use to see if a value is blank or not. </param>
        /// <returns> Either an empty or the generated value of the type <seealso cref="String"/>. </returns>
        protected string CheckBlank<Func>(Func func, int blanks_percentage) where Func : Delegate
        {
            if (blanks_percentage is not 0 && Random.Next(0, 100) < blanks_percentage)
            {
                return string.Empty;
            }

            return func?.DynamicInvoke()?.ToString() ?? "";
        }
    }
}
