using System.Collections.Generic;
using System;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using System.Linq;
using TableConverter.Services.DataGenerationHandlersWithControls;
using TableConverter.DataGeneration;

namespace TableConverter.Services
{
    public class DataGenerationTypesService : DataGenerationGenHandler
    {
        public IReadOnlyList<DataGenerationType> Types { get; } = new DataGenerationType[]
        {
            new("Row Number", "Basic",
                "Generates a row number. For example 1, 2, 3.",
                typeof(DataGenerationRowNumberHandler)),
            new("Character Sequence", "Advanced",
                "Creates a sequence of characters, numbers, and symbols.",
                typeof(DataGenerationCharacterSequenceHandlerWithControls)),
            new("Date Time", "Basic",
                "Generates a date and time between a range and in a certain format.",
                typeof(DataGenerationDateTimeHandlerWithControls)),
            new("Hex Colour", "Basic",
                "Generates a random hex colour. For example #FFFFFF.",
                typeof(DataGenerationHexColourHandler)),
            new("Colour", "Basic",
                "Generates a random colour. For example Green, Blue, Yellow, etc.",
                typeof(DataGenerationColourHandler)),
            new("Custom List", "Advanced",
                "Generates a random item from a list of items.",
                typeof(DataGenerationCustomListHandlerWithControls)),
            new("Guid", "Basic",
                "Generates a random GUID (Globally Unique Identifier).",
                typeof(DataGenerationGuidHandler)),
            new("IP Address", "Web",
                "Generates random IPv4 or IPv6 addresses.",
                typeof(DataGenerationIPAddressHandlerWithControls)),
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
                typeof(DataGenerationNumberHandlerWithControls)),
            new("Nato Phonetic", "Advanced",
                "Generates a random word from the Nato Phonetic Alphabet.",
                typeof(DataGenerationNatoPhoneticHandler)),
            new("Gender", "Personal",
                "Generates a random gender. For example \'Male\' or \'M\'.",
                typeof(DataGenerationGenderHandlerWithControls)),
            new("First Name", "Personal",
                "Generates a random first name.",
                typeof(DataGenerationFirstNameHandlerWithControls)),
            new("Last Name", "Personal",
                "Generates a random last name.",
                typeof(DataGenerationLastNameHandlerWithControls)),
            new("Full Name", "Personal",
                "Generates a random first and last name together.",
                typeof(DataGenerationFirstLastNameHandlerWithControls)),
            new("Title", "Personal",
                "Generates a random title. For example Mr, Mrs, Dr.",
                typeof(DataGenerationTitleHandler)),
            new("Shirt Size", "Personal",
                "Generates a random shirt size. For example S, M, L, or XL.",
                typeof(DataGenerationShirtSizesHandlerWithControls)),
            new("Website URL", "Web",
                "Generates a random website URL.",
                typeof(DataGenerationWebsiteUrlHandler)),
            new("Company Name", "Business",
                "Generates a random company name.",
                typeof(DataGenerationCompanyNameHandlerWithControls)),
            new("Mobile Brand", "Products",
                "Generates a random mobile brand. For example Apple, Samsung.",
                typeof(DataGenerationMobileBrandsHandler)),
            new("Mobile Model", "Products",
                "Generates a random mobile model.",
                typeof(DataGenerationMobileBrandsModelsHandlerWithControls)),
            new("Mobile OS", "Products",
                "Generates a random mobile operating system. For example iOS, Android.",
                typeof(DataGenerationMobileOsHandler)),
            new("Movie Title", "Entertainment",
                "Generates a random movie title. For example Plane, Fast X.",
                typeof(DataGenerationMoviesHandlerWithControls)),
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
        }.OrderBy(val => val.Name).ToArray();

        /// <summary>
        /// Gets the generation type by the name of the generator.
        /// </summary>
        /// <param name="name"> The name of the generator. </param>
        /// <returns> The generation type instance. </returns>
        public DataGenerationType GetByName(string name) => Types.First(val => val.Name == name);

        /// <summary>
        /// Creates a new instance of the generation handler by the name of the generator.
        /// </summary>
        /// <param name="name"> The name of the generator. </param>
        /// <returns> The newly created instance of the generation handler. </returns>
        /// <exception cref="Exception"> Thrown if the function doesnt manage to create an instance of the handler. </exception>
        public IDataGenerationTypeHandler GetHandlerByName(string name)
        {
            IDataGenerationTypeHandler? handler=(IDataGenerationTypeHandler?)Activator.CreateInstance(GetByName(name).GeneratorType);

            if (handler is null)
            {
                throw new Exception($"Handler for {name} not found.");
            }

            return handler;
        }
    }
}
