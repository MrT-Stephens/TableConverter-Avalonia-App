using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerJsonInput : ConverterHandlerInputAbstract<ConverterHandlerJsonInputOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            try
            {
                switch (Options!.SelectedJsonFormatType)
                {
                    case "Array of Objects":
                        {
                            List<Dictionary<string, object>>? json_objects = new();

                            json_objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);

                            if (json_objects is not null)
                            {
                                foreach (var json_object in json_objects)
                                {
                                    foreach (var key in json_object.Keys)
                                    {
                                        if (!headers.Contains(key))
                                        {
                                            headers.Add(key);
                                        }
                                    }

                                    rows.Add(headers.ConvertAll(header => json_object.ContainsKey(header) ? json_object[header]?.ToString() ?? "" : string.Empty).ToArray());
                                }
                            }

                            break;
                        }
                    case "2D Arrays":
                        {
                            List<List<object>>? json_arrays = new();

                            json_arrays = JsonConvert.DeserializeObject<List<List<object>>>(text);

                            if (json_arrays is not null)
                            {
                                headers.AddRange(json_arrays[0].ConvertAll(value => value?.ToString() ?? ""));

                                for (long i = 1; i < json_arrays.LongCount(); i++)
                                {
                                    rows.Add(json_arrays[(int)i].ConvertAll(value => value?.ToString() ?? "").ToArray());
                                }
                            }

                            break;
                        }
                    case "Column Arrays":
                        {
                            List<Dictionary<string, string[]>>? json_objects = new();

                            json_objects = JsonConvert.DeserializeObject<List<Dictionary<string, string[]>>>(text);

                            if (json_objects is not null)
                            {
                                for (long i = 0; i < json_objects.LongCount(); i++)
                                {
                                    headers.Add(json_objects[(int)i].Keys.First());

                                    for (long j = 0; j < json_objects[(int)i].Values.First().LongCount(); j++)
                                    {
                                        if (i == 0)
                                        {
                                            rows.Add(new string[json_objects.LongCount()]);
                                        }

                                        rows[(int)j][i] = json_objects[(int)i].Values.First()[(int)j];
                                    }
                                }
                            }

                            break;
                        }
                    case "Keyed Arrays":
                        {
                            List<Dictionary<string, string[]>>? json_objects = new();

                            json_objects = JsonConvert.DeserializeObject<List<Dictionary<string, string[]>>>(text);

                            if (json_objects is not null)
                            {
                                headers.AddRange(json_objects[0].Values.First().Select(value => value?.ToString() ?? ""));

                                for (long i = 1; i < json_objects.LongCount(); i++)
                                {
                                    rows.Add(json_objects[(int)i].Values.First().ToArray());
                                }
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid JSON format. Please check the JSON format and try again.", ex);
            }

            return new TableData(headers, rows);
        }
    }
}
