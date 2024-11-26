using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerJsonOutput : ConverterHandlerOutputAbstract<ConverterHandlerJsonOutputOptions>
    {
        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            try
            {
                switch (Options!.SelectedJsonFormatType)
                {
                    case "Array of Objects":
                    {
                        var jsonObjects = new Dictionary<string, object>[rows.Length];

                        for (var i = 0; i < rows.Length; i++)
                        {
                            jsonObjects[i] = new Dictionary<string, object>();

                            for (var j = 0; j < headers.Length; j++)
                            {
                                jsonObjects[i].Add(headers[j].Replace(' ', '_'), rows[i][j]);
                            }
                        }

                        return Result<string>.Success(JsonConvert.SerializeObject(jsonObjects,
                            Options!.MinifyJson ? Formatting.None : Formatting.Indented));
                    }
                    case "2D Arrays":
                    {
                        var jsonArray = new string[rows.Length + 1][];

                        jsonArray[0] = headers.Select(c => c.Replace(' ', '_')).ToArray();

                        for (var i = 0; i < rows.Length; i++)
                        {
                            jsonArray[i + 1] = rows[i];
                        }

                        return Result<string>.Success(JsonConvert.SerializeObject(jsonArray,
                            Options!.MinifyJson ? Formatting.None : Formatting.Indented));
                    }
                    case "Column Arrays":
                    {
                        var jsonObjects = new Dictionary<string, string[]>[headers.Length];

                        for (var i = 0; i < headers.Length; i++)
                        {
                            jsonObjects[i] = new Dictionary<string, string[]>
                            {
                                { headers[i].Replace(' ', '_'), rows.Select(row => row[i]).ToArray() }
                            };
                        }

                        return Result<string>.Success(JsonConvert.SerializeObject(jsonObjects,
                            Options!.MinifyJson ? Formatting.None : Formatting.Indented));
                    }
                    case "Keyed Arrays":
                    {
                        var jsonObjects = new Dictionary<long, string[]>[rows.Length + 1];

                        jsonObjects[0] = new Dictionary<long, string[]>
                        {
                            { 0, headers.Select(c => c.Replace(' ', '_')).ToArray() }
                        };

                        for (var i = 0; i < rows.Length; i++)
                        {
                            jsonObjects[i + 1] = new Dictionary<long, string[]>
                            {
                                { i + 1, rows[i] }
                            };
                        }

                        return Result<string>.Success(JsonConvert.SerializeObject(jsonObjects,
                            Options!.MinifyJson ? Formatting.None : Formatting.Indented));
                    }
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message);
            }

            return Result<string>.Failure("Unsupported json format");
        }
    }
}
