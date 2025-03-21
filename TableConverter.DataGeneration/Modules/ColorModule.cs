using System.Globalization;
using TableConverter.DataGeneration.Exceptions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum CssFunctionEnum
{
    Rgb,
    Rgba,
    Hsl,
    Hsla,
    Hwb,
    Cmyk,
    Lab,
    Lch,
    Color
}

public enum CssSpaceEnum
{
    Srgb,
    DisplayP3,
    Rec2020,
    A98Rgb,
    ProphotoRgb
}

public enum ColorFormatEnum
{
    Css,
    Binary,
    Decimal
}

public enum RgbColorFormatEnum
{
    Css = ColorFormatEnum.Css,
    Binary = ColorFormatEnum.Binary,
    Decimal = ColorFormatEnum.Decimal,
    Hex
}

public class ColorModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    private static readonly IReadOnlyDictionary<CssFunctionEnum, string> CssFunctions =
        new Dictionary<CssFunctionEnum, string>
        {
            { CssFunctionEnum.Rgb, "rgb" },
            { CssFunctionEnum.Rgba, "rgba" },
            { CssFunctionEnum.Hsl, "hsl" },
            { CssFunctionEnum.Hsla, "hsla" },
            { CssFunctionEnum.Hwb, "hwb" },
            { CssFunctionEnum.Cmyk, "cmyk" },
            { CssFunctionEnum.Lab, "lab" },
            { CssFunctionEnum.Lch, "lch" },
            { CssFunctionEnum.Color, "color" }
        };

    private static readonly IReadOnlyDictionary<CssSpaceEnum, string> CssSpaces =
        new Dictionary<CssSpaceEnum, string>
        {
            { CssSpaceEnum.Srgb, "srgb" },
            { CssSpaceEnum.DisplayP3, "display-p3" },
            { CssSpaceEnum.Rec2020, "rec2020" },
            { CssSpaceEnum.A98Rgb, "a98-rgb" },
            { CssSpaceEnum.ProphotoRgb, "prophoto-rgb" }
        };

    public override string ModuleName => "Color";

    public virtual string Human()
    {
        return Randomizer.GetRandomElement(Locale.Color.Value.HumanName);
    }

    public virtual string Space()
    {
        return Randomizer.GetRandomElement(Locale.Color.Value.Space);
    }

    public virtual CssFunctionEnum CssSupportedFunctionEnum()
    {
        return Randomizer.GetRandomElement(Enum.GetValues<CssFunctionEnum>());
    }

    public virtual string CssSupportedFunction()
    {
        return CssFunctions[CssSupportedFunctionEnum()];
    }

    public virtual CssSpaceEnum CssSupportedSpaceEnum()
    {
        return Randomizer.GetRandomElement(Enum.GetValues<CssSpaceEnum>());
    }

    public virtual string CssSupportedSpace()
    {
        return CssSpaces[CssSupportedSpaceEnum()];
    }

    public virtual string Rgb(
        RgbColorFormatEnum colorFormat = RgbColorFormatEnum.Hex,
        HexCasing hexadecimalCasing = HexCasing.Lower,
        string prefix = "#",
        bool includeAlpha = false)
    {
        if (colorFormat == RgbColorFormatEnum.Hex)
            return Randomizer.Hexadecimal(includeAlpha ? 8 : 6, hexadecimalCasing, prefix);

        var values = new List<double>(includeAlpha ? 4 : 3);

        values.AddRange(Enumerable.Range(0, 3).Select(_ => (double)Randomizer.Number(0, 255)));

        if (includeAlpha)
            values.Add(Randomizer.Float());

        if (colorFormat == RgbColorFormatEnum.Decimal)
            return ToDecimalString(values.ToArray());

        if (colorFormat == RgbColorFormatEnum.Binary)
            return string.Join(" ", ToBinaryStrings(values.ToArray()));

        if (colorFormat == RgbColorFormatEnum.Css)
            return FormatCss(values.ToArray(), includeAlpha ? CssFunctionEnum.Rgba : CssFunctionEnum.Rgb);

        return FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format");
    }

    public virtual string Cmyk(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        var values = new List<double>(4);

        values.AddRange(Enumerable.Range(0, 4).Select(_ => (double)Randomizer.Float()));

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(), CssFunctionEnum.Cmyk),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    public virtual string Hsl(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal, bool includeAlpha = false)
    {
        var values = new List<double>(includeAlpha ? 4 : 3)
        {
            Randomizer.Number(0, 360), // Hue
            Randomizer.Float(), // Saturation
            Randomizer.Float() // Lightness
        };

        if (includeAlpha)
            values.Add(Randomizer.Float());

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(),
                includeAlpha ? CssFunctionEnum.Hsla : CssFunctionEnum.Hsl),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    public virtual string Hwb(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        var values = new List<double>(3)
        {
            Randomizer.Number(0, 360), // Hue
            Randomizer.Float(), // Whiteness
            Randomizer.Float() // Blackness
        };

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(), CssFunctionEnum.Hwb),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    public virtual string Lab(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        var values = new List<double>(3)
        {
            Randomizer.Float(), // Lightness
            Randomizer.Float(-100f, 100f), // A
            Randomizer.Float(-100f, 100f) // B
        };

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(), CssFunctionEnum.Lab),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    public virtual string Lch(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        var values = new List<double>(3)
        {
            Randomizer.Float(), // Lightness
            Randomizer.Float(0f, 230f),
            Randomizer.Float(0f, 230f)
        };

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(), CssFunctionEnum.Lch),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    public virtual string ColorByCssSpace(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal,
        CssSpaceEnum colorSpace = CssSpaceEnum.Srgb)
    {
        var values = new List<double>(3)
        {
            Randomizer.Float(),
            Randomizer.Float(),
            Randomizer.Float()
        };

        return colorFormat switch
        {
            ColorFormatEnum.Decimal => ToDecimalString(values.ToArray()),
            ColorFormatEnum.Binary => string.Join(" ", ToBinaryStrings(values.ToArray())),
            ColorFormatEnum.Css => FormatCss(values.ToArray(), CssFunctionEnum.Color, colorSpace),
            _ => FakerArgumentException.CreateException<string>(colorFormat, "Invalid color format")
        };
    }

    /// <summary>
    ///     Formats an array of color values into a CSS-compatible string representation.
    /// </summary>
    /// <param name="values">The array of color values to format.</param>
    /// <param name="cssFunction">The CSS function to use for formatting. Defaults to RGB.</param>
    /// <param name="space">The color space to use for the CSS function. Defaults to sRGB.</param>
    /// <returns>A string representing the formatted CSS color function.</returns>
    private static string FormatCss(
        double[] values,
        CssFunctionEnum cssFunction = CssFunctionEnum.Rgb,
        CssSpaceEnum space = CssSpaceEnum.Srgb)
    {
        return cssFunction switch
        {
            CssFunctionEnum.Rgba =>
                $"rgba({FormatValue(values[0])}, {FormatValue(values[1])}, {FormatValue(values[2])}, {FormatValue(values[3])})",
            CssFunctionEnum.Color =>
                $"color({CssSpaces[space]} {FormatValue(values[0])} {FormatValue(values[1])} {FormatValue(values[2])})",
            CssFunctionEnum.Cmyk =>
                $"cmyk({ToPercentage(values[0])}%, {ToPercentage(values[1])}%, {ToPercentage(values[2])}%, {ToPercentage(values[3])}%)",
            CssFunctionEnum.Hsl =>
                $"hsl({FormatValue(values[0])}deg {ToPercentage(values[1])}% {ToPercentage(values[2])}%)",
            CssFunctionEnum.Hsla =>
                $"hsl({FormatValue(values[0])}deg {ToPercentage(values[1])}% {ToPercentage(values[2])}% / {FormatValue(values[3])})",
            CssFunctionEnum.Hwb =>
                $"hwb({FormatValue(values[0])} {ToPercentage(values[1])}% {ToPercentage(values[2])}%)",
            CssFunctionEnum.Lab =>
                $"lab({ToPercentage(values[0])}% {FormatValue(values[1])} {FormatValue(values[2])})",
            CssFunctionEnum.Lch =>
                $"lch({ToPercentage(values[0])}% {FormatValue(values[1])} {FormatValue(values[2])})",
            CssFunctionEnum.Rgb =>
                $"rgb({FormatValue(values[0])}, {FormatValue(values[1])}, {FormatValue(values[2])})",
            _ => FakerArgumentException.CreateException<string>(cssFunction, "Invalid CSS function type")
        };

        // Helper function to convert values to int or round floats to 2dp
        string FormatValue(double value)
        {
            return value % 1 == 0
                ? ((int)value).ToString(CultureInfo.InvariantCulture) // Integer
                : Math.Round(value, 2).ToString("F2", CultureInfo.InvariantCulture);
        } // Float to 2dp
    }

    /// <summary>
    ///     Converts an array of double values into a formatted string representation.
    ///     Each value is either converted to an integer if it has no fractional part,
    ///     or rounded to two decimal places if it is a floating-point number.
    /// </summary>
    /// <param name="values">The array of double values to convert.</param>
    /// <returns>A formatted string representing the array, with values separated by commas.</returns>
    private static string ToDecimalString(double[] values)
    {
        return $"[{string.Join(", ", values.Select(FormatValue))}]";

        // Helper function to convert values to int or round floats to 2dp
        string FormatValue(double value)
        {
            return value % 1 == 0
                ? ((int)value).ToString(CultureInfo.InvariantCulture) // Integer
                : Math.Round(value, 2).ToString("F2", CultureInfo.InvariantCulture);
        } // Float to 2dp
    }

    /// <summary>
    ///     Converts an array of doubles to an array of binary string representations.
    ///     For integers, it converts them to binary with leading zeroes.
    ///     For floating-point numbers, it uses IEEE 754 single-precision representation.
    /// </summary>
    /// <param name="values">The array of doubles to convert.</param>
    /// <returns>An array of binary string representations.</returns>
    private static string[] ToBinaryStrings(double[] values)
    {
        return values.Select(value =>
        {
            // Check if the value is a floating-point number
            if (value % 1 != 0)
            {
                // Convert to IEEE 754 single-precision (32-bit) floating-point binary
                var bytes = BitConverter.GetBytes((float)Math.Round(value, 2));

                return string.Join("", bytes.Reverse() // Ensure big-endian order
                    .Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))); // Format each byte as binary
            }

            // For integers, convert directly to binary with leading zeroes
            return Convert.ToString((int)value, 2).PadLeft(8, '0');
        }).ToArray();
    }

    /// <summary>
    ///     Converts a value to a percentage by multiplying it by 100 and rounding.
    /// </summary>
    /// <param name="value">The value to convert to a percentage.</param>
    /// <returns>The percentage value as an integer.</returns>
    private static int ToPercentage(double value)
    {
        return (int)Math.Round(value * 100);
    }
}