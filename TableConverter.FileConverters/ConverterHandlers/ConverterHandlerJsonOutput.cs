using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerJsonOutput : ConverterHandlerOutputAbstract<ConverterHandlerJsonOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            switch (Options!.SelectedJsonFormatType)
            {
                case "Array of Objects":
                    {
                        Dictionary<string, object>[] json_objects = new Dictionary<string, object>[rows.LongLength];

                        for (long i = 0; i < rows.LongLength; i++)
                        {
                            json_objects[i] = new Dictionary<string, object>();

                            for (long j = 0; j < headers.LongLength; j++)
                            {
                                json_objects[i].Add(headers[j].Replace(' ', '_'), rows[i][j]);
                            }
                        }

                        return JsonConvert.SerializeObject(json_objects, Options!.MinifyJson ? Formatting.None : Formatting.Indented);
                    }
                case "2D Arrays":
                    {
                        string[][] json_array = new string[rows.LongLength + 1][];

                        json_array[0] = headers.Select(c => c.Replace(' ', '_')).ToArray();

                        for (long i = 0; i < rows.LongLength; i++)
                        {
                            json_array[i + 1] = rows[i];
                        }

                        return JsonConvert.SerializeObject(json_array, Options!.MinifyJson ? Formatting.None : Formatting.Indented);
                    }
                case "Column Arrays":
                    {
                        Dictionary<string, string[]>[] json_objects = new Dictionary<string, string[]>[headers.LongLength];

                        for (long i = 0; i < headers.LongLength; i++)
                        {
                            json_objects[i] = new Dictionary<string, string[]>()
                            {
                                { headers[i].Replace(' ', '_'), rows.Select(row => row[i]).ToArray() }
                            };
                        }

                        return JsonConvert.SerializeObject(json_objects, Options!.MinifyJson ? Formatting.None : Formatting.Indented);
                    }
                case "Keyed Arrays":
                    {
                        Dictionary<long, string[]>[] json_objects = new Dictionary<long, string[]>[rows.LongLength + 1];

                        json_objects[0] = new Dictionary<long, string[]>
                        {
                            { 0, headers.Select(c => c.Replace(' ', '_')).ToArray() }
                        };

                        for (long i = 0; i < rows.LongLength; i++)
                        {
                            json_objects[i + 1] = new Dictionary<long, string[]>()
                            {
                                { i + 1, rows[i] }
                            };
                        }

                        return JsonConvert.SerializeObject(json_objects, Options!.MinifyJson ? Formatting.None : Formatting.Indented);
                    }
            }

            return string.Empty;
        }
    }
}
