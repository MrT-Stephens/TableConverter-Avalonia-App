﻿using Avalonia.Controls;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationDateTimeHandlerWithControls : DataGenerationDateTimeHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {

        }
    }
}
