﻿using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationColourHandler : DataGenerationTypeHandlerAbstract
    {
        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() =>
                    {
                        ConsoleColor[] colorsArray = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
                        return colorsArray.GetValue(Random.Next(0, colorsArray.Length))?.ToString();
                    }
                    , blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}