﻿using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public interface IConverterHanderInput
    {
        public Collection<Control>? Controls { get; set; }

        public void InitializeControls();

        public Task<string> ReadFileAsync(IStorageFile storage_file);

        public Task<(List<string>, List<string[]>)> ReadTextAsync(string text);
    }
}