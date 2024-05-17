using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Services.DataGenerationHandlerServices;

namespace TableConverter.Services
{
    internal static class DataGenerationTypesService
    {
        public static Task<Dictionary<DataGenerationType, Type>> GetDataGenerationTypesAsync()
        {
            return Task.FromResult(new Dictionary<DataGenerationType, Type>()
            {
                { 
                    new DataGenerationType("Row Number", "Basic", 
                        "Generates a row number. For example 1, 2, 3."), 
                        typeof(DataGenerationRowNumberHandler)
                },
                {
                    new DataGenerationType("Character Sequence", "Advanced",
                        "Creates a sequence of characters, numbers, and symbols."),
                        typeof(DataGenerationCharacterSequenceHandler)
                },
                {
                    new DataGenerationType("Date Time", "Basic",
                        "Generates a date and time between a range and in a certain format."),
                        typeof(DataGenerationDateTimeHandler)
                },
                {
                    new DataGenerationType("Hex Colour", "Basic",
                        "Generates a random hex colour. For example #FFFFFF."),
                        typeof(DataGenerationHexColourHandler)
                },
                {
                    new DataGenerationType("Colour", "Basic",
                        "Generates a random colour. For example Green, Blue, Yellow, etc."),
                        typeof(DataGenerationColourHandler)
                },
                {
                    new DataGenerationType("Custom List", "Advanced",
                        "Generates a random item from a list of items."),
                        typeof(DataGenerationCustomListHandler)
                },
                {
                    new DataGenerationType("Guid", "Basic",
                        "Generates a random GUID (Globally Unique Identifier)."),
                        typeof(DataGenerationGuidHandler)
                },
                {
                    new DataGenerationType("IP Address", "Web",
                        "Generates random IPv4 or IPv6 addresses."),
                        typeof(DataGenerationIPAddressHandler)
                },
                {
                    new DataGenerationType("MAC Address", "Web",
                        "Generates random MAC addresses."),
                        typeof(DataGenerationMacAddressHandler)
                },
                {
                    new DataGenerationType("App Version", "Basic",
                        "Generates a random app version. For example 1.0.0."),
                        typeof(DataGenerationAppVersionHandler)
                },
                {
                    new DataGenerationType("Latitude", "Location",
                        "Generates random latitude coordinate."),
                        typeof(DataGenerationLatitudeHandler)
                },
                {
                    new DataGenerationType("Longitude", "Location",
                        "Generates random longitude coordinate."),
                        typeof(DataGenerationLongitudeHandler)
                },
                {
                    new DataGenerationType("Country Code", "Location",
                        "Generates random country code. For example US, GB, FR."),
                        typeof(DataGenerationCountryCodeHandler)
                },
                {
                    new DataGenerationType("Country", "Location",
                        "Generates random country name. For example United States, United Kingdom, France."),
                        typeof(DataGenerationCountryHandler)
                },
                {
                    new DataGenerationType("Number", "Basic",
                        "Generates a random number between a range."),
                        typeof(DataGenerationNumberHandler)
                },
                {
                    new DataGenerationType("Nato Phonetic", "Advanced",
                        "Generates a random word from the Nato Phonetic Alphabet."),
                        typeof(DataGenerationNatoPhoneticHandler)
                },
                {
                    new DataGenerationType("Gender", "Personal",
                        "Generates a random gender. For example \'Male\' or \'M\'."),
                        typeof(DataGenerationGenderHandler)
                },
                {
                    new DataGenerationType("First Name", "Personal",
                        "Generates a random first name."),
                        typeof(DataGenerationFirstNameHandler)
                },
                {
                    new DataGenerationType("Last Name", "Personal",
                        "Generates a random last name."),
                        typeof(DataGenerationLastNameHandler)
                },
                {
                    new DataGenerationType("Full Name", "Personal",
                        "Generates a random first and last name together."),
                        typeof(DataGenerationFirstLastNameHandler)
                },
                {
                    new DataGenerationType("Title", "Personal",
                        "Generates a random title. For example Mr, Mrs, Dr."),
                        typeof(DataGenerationTitleHandler)
                },
                {
                    new DataGenerationType("Shirt Size", "Personal",
                        "Generates a random shirt size. For example S, M, L, or XL."),
                        typeof(DataGenerationShirtSizesHandler)
                },
                {
                    new DataGenerationType("Website URL", "Web",
                        "Generates a random website URL."),
                        typeof(DataGenerationWebsiteUrlHandler)
                },
                {
                    new DataGenerationType("Company Name", "Business",
                        "Generates a random company name."),
                        typeof(DataGenerationCompanyNameHandler)
                },
                {
                    new DataGenerationType("Mobile Brand", "Products",
                        "Generates a random mobile brand. For example Apple, Samsung."),
                        typeof(DataGenerationMobileBrandsHandler)
                },
                {
                    new DataGenerationType("Mobile Model", "Products",
                        "Generates a random mobile model."),
                        typeof(DataGenerationMobileBrandsModelsHandler)
                },
                {
                    new DataGenerationType("Mobile OS", "Products",
                        "Generates a random mobile operating system. For example iOS, Android."),
                        typeof(DataGenerationMobileOsHandler)
                },
                {
                    new DataGenerationType("Movie Title", "Entertainment",
                        "Generates a random movie title. For example Plane, Fast X."),
                        typeof(DataGenerationMoviesHandler)
                },
                {
                    new DataGenerationType("Movie Genre", "Entertainment",
                        "Generates a random music genre. For example Action, Sci-Fi."),
                        typeof(DataGenerationMovieGenresHandler)
                }
            }.OrderBy(val => val.Key.Name).ToDictionary(entry => entry.Key, entry => entry.Value));
        }

        public static Task<TableData> GetDataGenerationDataAsync(DataGenerationField[] data_generation_fields, long number_of_rows) 
        {
            return Task.Run(async () =>
            {
                List<string> headers = new List<string>();
                List<string[]> rows = new List<string[]>();

                for (long i = 0; i < number_of_rows; i++)
                {
                    rows.Add(new string[data_generation_fields.LongLength]);
                }

                for (long i = 0; i < data_generation_fields.LongLength; i++)
                {
                    if (data_generation_fields[i].TypeHandler is not null)
                    {
                        headers.Add((!string.IsNullOrEmpty(data_generation_fields[i].Name)) ? data_generation_fields[i].Name : data_generation_fields[i].Type);

                        string[] column = await data_generation_fields[i].TypeHandler!.GenerateData(number_of_rows, data_generation_fields[i].BlankPercentage.GetValueOrDefault());

                        for (long j = 0; j < column.LongLength; j++)
                        {
                            rows[(int)j][i] = column[j];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                return new TableData(headers, rows);
            });
        }
    }
}
