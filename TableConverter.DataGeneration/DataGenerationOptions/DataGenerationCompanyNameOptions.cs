﻿namespace TableConverter.DataGeneration.DataGenerationOptions
{
    public class DataGenerationCompanyNameOptions : DataGenerationBaseOptions
    {
        public string[] CountryCodes { get; set; } = Array.Empty<string>();

        public string CountryCode { get; set; } = string.Empty;
    }
}