using System;
using System.Numerics;
using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Number",
    "Module for generating random numeric data, including integers, floating-point numbers, and special formats like binary and roman numerals.",
    "DataGenerationNumberIcon")]
public class NumberAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : NumberModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Integer",
        "Generates a random integer between the specified minimum and maximum values. Default: minNumber = int.MinValue, maxNumber = int.MaxValue.")]
    public override string Integer(int minNumber = int.MinValue, int maxNumber = int.MaxValue)
    {
        return base.Integer(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Long",
        "Generates a random long integer between the specified minimum and maximum values. Default: minNumber = long.MinValue, maxNumber = long.MaxValue.")]
    public override string Long(long minNumber = long.MinValue, long maxNumber = long.MaxValue)
    {
        return base.Long(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("BigInteger",
        "Generates a random BigInteger. No fixed range is defined for BigInteger, as it can represent any arbitrarily large or small number.")]
    public override string BigInteger(string minNumber = "0", string maxNumber = "9999999999999999999999999999")
    {
        return base.BigInteger(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Float",
        "Generates a random floating-point number between the specified minimum and maximum values. Default: minNumber = float.MinValue, maxNumber = float.MaxValue. The result is rounded to the specified number of decimal places.")]
    public override string Float(float minNumber = float.MinValue, float maxNumber = float.MaxValue,
        int decimalPlaces = 2)
    {
        return base.Float(minNumber, maxNumber, decimalPlaces);
    }

    [DataGenerationModuleMethod("Double",
        "Generates a random double-precision floating-point number between the specified minimum and maximum values. Default: minNumber = double.MinValue, maxNumber = double.MaxValue. The result is rounded to the specified number of decimal places.")]
    public override string Double(double minNumber = double.MinValue, double maxNumber = double.MaxValue,
        int decimalPlaces = 2)
    {
        return base.Double(minNumber, maxNumber, decimalPlaces);
    }

    [DataGenerationModuleMethod("Decimal",
        "Generates a random decimal number between the specified minimum and maximum values. Default: minNumber = decimal.MinValue, maxNumber = decimal.MaxValue. The result is rounded to the specified number of decimal places.")]
    public override string Decimal(decimal minNumber = decimal.MinValue, decimal maxNumber = decimal.MaxValue,
        int decimalPlaces = 2)
    {
        return base.Decimal(minNumber, maxNumber, decimalPlaces);
    }

    [DataGenerationModuleMethod("Binary",
        "Generates a random binary number (string representation) between the specified minimum and maximum values. Default: minNumber = 0, maxNumber = 255.")]
    public override string Binary(int minNumber = 0, int maxNumber = 255)
    {
        return base.Binary(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Octal",
        "Generates a random octal number (string representation) between the specified minimum and maximum values. Default: minNumber = 0, maxNumber = 255.")]
    public override string Octal(int minNumber = 0, int maxNumber = 255)
    {
        return base.Octal(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Hexadecimal",
        "Generates a random hexadecimal number (string representation) between the specified minimum and maximum values. Default: minNumber = 0, maxNumber = 255.")]
    public override string Hexadecimal(int minNumber = 0, int maxNumber = 255)
    {
        return base.Hexadecimal(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Roman Numeral",
        "Generates a random Roman numeral between the specified minimum and maximum values. Default: minNumber = 1, maxNumber = 3999.")]
    public override string RomanNumeral(int minNumber = 1, int maxNumber = 3999)
    {
        return base.RomanNumeral(minNumber, maxNumber);
    }

    [DataGenerationModuleMethod("Percent",
        "Generates a random percentage between the specified minimum and maximum values. Default: minNumber = 0, maxNumber = 100. The result is rounded to the specified number of decimal places, and the percent symbol can optionally be included.")]
    public override string Percent(int minNumber = 0, int maxNumber = 100, int decimalPlaces = 2,
        bool includeSymbol = true)
    {
        return base.Percent(minNumber, maxNumber, decimalPlaces, includeSymbol);
    }

    [DataGenerationModuleMethod("Prime Number",
        "Generates a random prime number between the specified minimum and maximum values. Default: minNumber = 2, maxNumber = 100.")]
    public override long PrimeNumber(long minNumber = 1, long maxNumber = long.MaxValue)
    {
        return base.PrimeNumber(minNumber, maxNumber);
    }
}