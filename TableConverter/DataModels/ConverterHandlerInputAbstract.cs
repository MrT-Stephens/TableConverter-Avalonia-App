﻿using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    internal abstract class ConverterHandlerInputAbstract : IConverterHanderInput
    {
        public Collection<Control>? Controls { get; set; }

        public virtual async Task<string> ReadFileAsync(IStorageFile storage_file)
        {
            using (var reader = new System.IO.StreamReader(await storage_file.OpenReadAsync()))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public abstract void InitializeControls();

        public abstract Task<TableData> ReadTextAsync(string text);
    }
}