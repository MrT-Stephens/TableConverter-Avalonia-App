using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.Services;

namespace TableConverter.DataGeneration.DataModels
{
    public abstract class DataGenerationTypeHandlerAbstract<T> : IDataGenerationTypeHandler where T : DataGenerationBaseOptions
    {
        /// <summary>
        /// The options type. Used internally by <seealso cref="DataGenerationTypeHandlerAbstract{T}"/>, but can be used externally.
        /// </summary>
        public Type OptionsType { get; init; }

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
            OptionsType = typeof(T);

            Random = new Random((int)DateTime.Now.ToBinary());
        }

        /// <summary>
        /// The generate data function that can be called by anyone.
        /// Will generate data based on the number of rows, options, and blanks percentage.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="options"> The options for the data generation object. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generater will generate. </param>
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        /// <exception cref="ArgumentException"> Throws if the OptionsType is not equal to the type of the passed in options. </exception>
        /// <exception cref="ArgumentNullException"> Throws if the OptionsType is not the base options and the passed options is null. </exception>
        public string[] GenerateData(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            if (options is not null && 
                OptionsType != typeof(DataGenerationBaseOptions) && 
                OptionsType != options.GetType())
            {
                throw new ArgumentException($"The options must be of the type '{OptionsType}'");
            }
            else if (OptionsType != typeof(DataGenerationBaseOptions) && options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return GenerateDataOverride(rows, options as T, blanks_percentage);
        }

        /// <summary>
        /// The generate data function that can be called by anyone.
        /// Will generate data based on the number of rows, options, and blanks percentage asynchronously.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="options"> The options for the data generation object. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generater will generate. </param>
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        /// <exception cref="ArgumentException"> Throws if the OptionsType is not equal to the type of the passed in options. </exception>
        /// <exception cref="ArgumentNullException"> Throws if the OptionsType is not the base options and the passed options is null. </exception>
        public async Task<string[]> GenerateDataAsync(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage)
        {
            if (options is not null &&
                OptionsType != typeof(DataGenerationBaseOptions) &&
                OptionsType != options.GetType())
            {
                throw new ArgumentException($"The options must be of the type '{OptionsType}'");
            }
            else if (OptionsType != typeof(DataGenerationBaseOptions) && options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await Task.Run(() => GenerateDataOverride(rows, options as T, blanks_percentage));
        }

        /// <summary>
        /// The internal generate function which gets called by <seealso cref="GenerateData(int, IDataGenerationOptions?, ushort)"/>.
        /// This function will get implemented by the extended versions of <seealso cref="DataGenerationTypeHandlerAbstract{T}"/>.
        /// </summary>
        /// <param name="rows"> Number of rows to generate. </param>
        /// <param name="options"> The options for the data generation object. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generater will generate. </param>
        /// <returns> <seealso cref="String"/>[] of the generated values. </returns>
        protected abstract string[] GenerateDataOverride(int rows, T? options, ushort blanks_percentage);


        /// <summary>
        /// The check blanks function. Will see if it needs to generate a blank value or not.
        /// </summary>
        /// <typeparam name="Func"> The type of the function. Must be a delegate type. </typeparam>
        /// <param name="func"> The function that will get called, if the value is not a blank value. </param>
        /// <param name="blanks_percentage"> Percentage of blacks that the generater will use to see if a value is blank or not. </param>
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
