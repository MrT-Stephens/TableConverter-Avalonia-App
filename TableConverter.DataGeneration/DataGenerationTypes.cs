using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration
{
    public class DataGenerationTypes
    {
        public readonly IEnumerable<DataGenerationType> Types = new List<DataGenerationType>
        {
            new("Row Number", "Basic",
                "Generates a row number. For example 1, 2, 3.",
                typeof(DataGenerationRowNumberHandler)),
            new("Character Sequence", "Advanced",
                "Creates a sequence of characters, numbers, and symbols.",
                typeof(DataGenerationCharacterSequenceHandler)),
            new("Date Time", "Basic",
                "Generates a date and time between a range and in a certain format.",
                typeof(DataGenerationDateTimeHandler)),
            new("Hex Colour", "Basic",
                "Generates a random hex colour. For example #FFFFFF.",
                typeof(DataGenerationHexColourHandler)),
            new("Colour", "Basic",
                "Generates a random colour. For example Green, Blue, Yellow, etc.",
                typeof(DataGenerationColourHandler)),
            new("Custom List", "Advanced",
                "Generates a random item from a list of items.",
                typeof(DataGenerationCustomListHandler)),
            new("Guid", "Basic",
                "Generates a random GUID (Globally Unique Identifier).",
                typeof(DataGenerationGuidHandler)),
            new("IP Address", "Web",
                "Generates random IPv4 or IPv6 addresses.",
                typeof(DataGenerationIPAddressHandler)),
            new("MAC Address", "Web",
                "Generates random MAC addresses.",
                typeof(DataGenerationMacAddressHandler)),
            new("App Version", "Basic",
                "Generates a random app version. For example 1.0.0.",
                typeof(DataGenerationAppVersionHandler)),
            new("Latitude", "Location",
                "Generates random latitude coordinate.",
                typeof(DataGenerationLatitudeHandler)),
            new("Longitude", "Location",
                "Generates random longitude coordinate.",
                typeof(DataGenerationLongitudeHandler)),
            new("Country Code", "Location",
                "Generates random country code. For example US, GB, FR.",
                typeof(DataGenerationCountryCodeHandler)),
            new("Country", "Location",
                "Generates random country name. For example United States, United Kingdom, France.",
                typeof(DataGenerationCountryHandler)),
            new("Number", "Basic",
                "Generates a random number between a range.",
                typeof(DataGenerationNumberHandler)),
            new("Nato Phonetic", "Advanced",
                "Generates a random word from the Nato Phonetic Alphabet.",
                typeof(DataGenerationNatoPhoneticHandler)),
            new("Gender", "Personal",
                "Generates a random gender. For example \'Male\' or \'M\'.",
                typeof(DataGenerationGenderHandler)),
            new("First Name", "Personal",
                "Generates a random first name.",
                typeof(DataGenerationFirstNameHandler)),
            new("Last Name", "Personal",
                "Generates a random last name.",
                typeof(DataGenerationLastNameHandler)),
            new("Full Name", "Personal",
                "Generates a random first and last name together.",
                typeof(DataGenerationFirstLastNameHandler)),
            new("Title", "Personal",
                "Generates a random title. For example Mr, Mrs, Dr.",
                typeof(DataGenerationTitleHandler)),
            new("Shirt Size", "Personal",
                "Generates a random shirt size. For example S, M, L, or XL.",
                typeof(DataGenerationShirtSizesHandler)),
            new("Website URL", "Web",
                "Generates a random website URL.",
                typeof(DataGenerationWebsiteUrlHandler)),
            new("Company Name", "Business",
                "Generates a random company name.",
                typeof(DataGenerationCompanyNameHandler)),
            new("Mobile Brand", "Products",
                "Generates a random mobile brand. For example Apple, Samsung.",
                typeof(DataGenerationMobileBrandsHandler)),
            new("Mobile Model", "Products",
                "Generates a random mobile model.",
                typeof(DataGenerationMobileBrandsModelsHandler)),
            new("Mobile OS", "Products",
                "Generates a random mobile operating system. For example iOS, Android.",
                typeof(DataGenerationMobileOsHandler)),
            new("Movie Title", "Entertainment",
                "Generates a random movie title. For example Plane, Fast X.",
                typeof(DataGenerationMoviesHandler)),
            new("Movie Genre", "Entertainment",
                "Generates a random movie genre. For example Action, Sci-Fi.",
                typeof(DataGenerationMovieGenresHandler)),
            new("Song Name", "Entertainment",
                "Generates a random song name.",
                typeof(DataGenerationSongsHandler)),
            new("Song Genre", "Entertainment",
                "Generates a random song genre. For example Pop, Rock.",
                typeof(DataGenerationSongGenresHandler)),
            new("Song Artist", "Entertainment",
                "Generates a random song artist.",
                typeof(DataGenerationSongArtistsHandler)),
            new("Book Title", "Entertainment",
                "Generates a random book title.",
                typeof(DataGenerationBookTitleHandler)),
            new("Book Author", "Entertainment",
                "Generates a random book author.",
                typeof(DataGenerationBookAuthorHandler)),
            new("Book Publisher", "Entertainment",
                "Generates a random book publisher.",
                typeof(DataGenerationBookPublisherHandler)),
            new("Book ISBN", "Entertainment",
                "Generates a random book ISBN.",
                typeof(DataGenerationBookIsbnHandler)),
        }.OrderBy(val => val.Name).ToList();

        /// <summary>
        /// Gets the generation type by the name of the generator.
        /// </summary>
        /// <param name="name"> The name of the generator. </param>
        /// <returns> The generation type instance. </returns>
        public DataGenerationType GetByName(string name) => Types.First(val => val.Name == name);

        /// <summary>
        /// Gets all generation types by the category of the generator.
        /// </summary>
        /// <param name="category"> The name of the category. </param>
        /// <returns> <seealso cref="DataGenerationType"/>[] of all the generator types by a category. /> </returns>
        public DataGenerationType[] GetAllByCategory(string category) => Types.Where(val => val.Category == category).ToArray();

        /// <summary>
        /// Gets all categories distinctly from the generators.
        /// </summary>
        /// <returns> <seealso cref="String"/>[] of all the categories. </returns>
        public string[] GetAllCategories() => Types.Select(val => val.Category).Distinct().ToArray();

        /// <summary>
        /// Creates a new instance of the generation handler by the name of the generator.
        /// </summary>
        /// <param name="name"> The name of the generator. </param>
        /// <returns> The newly created instance of the generation handler. </returns>
        /// <exception cref="Exception"> Thrown if the function doesnt manage to create an instance of the handler. </exception>
        public IDataGenerationTypeHandler GetHandlerByName(string name)
        {
            IDataGenerationTypeHandler? handler = (IDataGenerationTypeHandler?)Activator.CreateInstance(GetByName(name).GeneratorType);

            if (handler is null)
            {
                throw new Exception($"Handler for {name} not found.");
            }

            return handler;
        }

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
