﻿using Avalonia.Controls;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationCustomListHandlerWithControls : DataGenerationCustomListHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {
            var sequence_text_box = new TextBox()
            {
                Watermark = "Example: item 1,item 2,item 3",
                MinWidth = 200,
            };

            sequence_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    Options!.ItemsList = text_box.Text ?? "";
                }
            };

            OptionsControls.Add(sequence_text_box);
        }
    }
}