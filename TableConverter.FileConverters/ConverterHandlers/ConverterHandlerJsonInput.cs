using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerJsonInput : ConverterHandlerInputAbstract<ConverterHandlerJsonInputOptions>
    {
        public override Result<TableData> ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            try
            {
                bool ValidateArrayOfObjects(List<Dictionary<string, object>> jsonObjects)
                {
                    if (jsonObjects.Count == 0)
                        return false;

                    var allKeys = jsonObjects.SelectMany(obj => obj.Keys).Distinct().ToList();
                    return jsonObjects.All(obj => allKeys.All(key => obj.ContainsKey(key)));
                }

                bool Validate2DArrays(List<List<object>> jsonArrays)
                {
                    if (jsonArrays.Count < 2)
                        return false;

                    var headerCount = jsonArrays[0].Count;
                    return jsonArrays.Skip(1).All(row => row.Count == headerCount);
                }

                bool ValidateColumnArrays(List<Dictionary<string, string[]>> jsonObjects)
                {
                    if (jsonObjects.Count == 0)
                        return false;

                    var columnLengths = jsonObjects.SelectMany(obj => obj.Values).Select(arr => arr.Length).Distinct().ToList();
                    return columnLengths.Count == 1; // All arrays must have the same length
                }

                bool ValidateKeyedArrays(List<Dictionary<string, string[]>> jsonObjects)
                {
                    if (jsonObjects.Count == 0)
                        return false;

                    var headerCount = jsonObjects[0].Values.First().Length;
                    return jsonObjects.All(obj => obj.Values.First().Length == headerCount);
                }
                
                switch (Options!.SelectedJsonFormatType)
                {
                    case "Array of Objects":
                        {
                            var jsonObjects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);
                            
                            if (jsonObjects is null || !ValidateArrayOfObjects(jsonObjects))
                                return Result<TableData>.Failure("Invalid or empty JSON schema for 'Array of Objects'.");
                            
                            foreach (var jsonObject in jsonObjects)
                            {
                                foreach (var key in jsonObject.Keys.Where(key => !headers.Contains(key)))
                                {
                                    headers.Add(key);
                                }

                                rows.Add(headers.ConvertAll(header => jsonObject.TryGetValue(header, out var value) ? value.ToString() ?? "" : string.Empty).ToArray());
                            }

                            break;
                        }
                    case "2D Arrays":
                        {
                            var jsonArrays = JsonConvert.DeserializeObject<List<List<object>>>(text);
                            
                            if (jsonArrays is null || !Validate2DArrays(jsonArrays))
                                return Result<TableData>.Failure("Invalid or empty JSON schema for '2D Arrays'.");
                            
                            headers.AddRange(jsonArrays[0].ConvertAll(value => value?.ToString() ?? ""));

                            for (int i = 1; i < jsonArrays.Count; i++)
                            {
                                rows.Add(jsonArrays[i].ConvertAll(value => value?.ToString() ?? "").ToArray());
                            }

                            break;
                        }
                    case "Column Arrays":
                        {
                            var jsonObjects = JsonConvert.DeserializeObject<List<Dictionary<string, string[]>>>(text);
                            
                            if (jsonObjects is null || !ValidateColumnArrays(jsonObjects))
                                return Result<TableData>.Failure("Invalid or empty JSON schema for 'Column Arrays'.");
                            
                            for (var i = 0; i < jsonObjects.Count; i++)
                            {
                                headers.Add(jsonObjects[i].Keys.First());

                                for (var j = 0; j < jsonObjects[i].Values.First().Length; j++)
                                {
                                    if (i == 0)
                                    {
                                        rows.Add(new string[jsonObjects.Count]);
                                    }

                                    rows[j][i] = jsonObjects[i].Values.First()[j];
                                }
                            }

                            break;
                        }
                    case "Keyed Arrays":
                        {
                            var jsonObjects = JsonConvert.DeserializeObject<List<Dictionary<string, string[]>>>(text);
                            
                            if (jsonObjects == null || !ValidateKeyedArrays(jsonObjects))
                                return Result<TableData>.Failure("Invalid or empty JSON schema for 'Keyed Arrays'.");
                            
                            headers.AddRange(jsonObjects[0].Values.First().Select(value => value?.ToString() ?? ""));

                            for (var i = 1; i < jsonObjects.Count; i++)
                            {
                                rows.Add(jsonObjects[i].Values.First().ToArray());
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                return Result<TableData>.Failure(ex.Message);
            }

            return Result<TableData>.Success(new TableData(headers, rows));
        }
    }
}
