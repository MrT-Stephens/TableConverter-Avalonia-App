using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Color",
    "Module for generating color-related data in various formats such as RGB, CMYK, HSL, and more.",
    "DataGenerationColorIcon")]
public class ColorAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ColorModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Human",
        "Generates a color name commonly associated with human color perception (e.g., 'red', 'blue'). Example: 'Red'")]
    public override string Human()
    {
        return base.Human();
    }

    [DataGenerationModuleMethod("Space",
        "Generates a color name from the ‘Space’ color palette, often used in design or visual arts. Example: 'Space Black'")]
    public override string Space()
    {
        return base.Space();
    }

    [DataGenerationModuleMethod("Css Supported Function",
        "Generates a CSS-supported color function, such as 'rgb()', 'rgba()', 'hsl()', etc. Example: 'rgb(255, 0, 0)'")]
    public override string CssSupportedFunction()
    {
        return base.CssSupportedFunction();
    }

    [DataGenerationModuleMethod("Css Supported Space",
        "Generates a CSS-supported color space value like 'srgb', 'display-p3', 'rec2020', etc. Example: 'srgb' or 'display-p3'")]
    public override string CssSupportedSpace()
    {
        return base.CssSupportedSpace();
    }

    [DataGenerationModuleMethod("Rgb",
        "Generates a color in RGB format. The format can be Hex, Decimal, etc. Example: 'rgb(255, 0, 0)' or '#ff0000'")]
    public override string Rgb(
        RgbColorFormatEnum colorFormat = RgbColorFormatEnum.Hex,
        HexCasing hexadecimalCasing = HexCasing.Lower,
        string prefix = "#",
        bool includeAlpha = false)
    {
        return base.Rgb(colorFormat, hexadecimalCasing, prefix, includeAlpha);
    }

    [DataGenerationModuleMethod("Cmyk",
        "Generates a color in CMYK format (Cyan, Magenta, Yellow, Key/Black). Example: '0.5, 0.0, 0.5, 0.0'")]
    public override string Cmyk(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        return base.Cmyk(colorFormat);
    }

    [DataGenerationModuleMethod("Hsl",
        "Generates a color in HSL format (Hue, Saturation, Lightness). Example: 'hsl(0, 100%, 50%)'")]
    public override string Hsl(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal, bool includeAlpha = false)
    {
        return base.Hsl(colorFormat, includeAlpha);
    }

    [DataGenerationModuleMethod("Hwb",
        "Generates a color in HWB format (Hue, White, Black). Example: 'hwb(360, 0%, 0%)'")]
    public override string Hwb(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        return base.Hwb(colorFormat);
    }

    [DataGenerationModuleMethod("Lab",
        "Generates a color in CIE Lab format, a color model based on human vision. Example: 'lab(50, 20, 30)'")]
    public override string Lab(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        return base.Lab(colorFormat);
    }

    [DataGenerationModuleMethod("Lch",
        "Generates a color in CIE LCH format (Lightness, Chroma, Hue). Example: 'lch(50, 30, 45)'")]
    public override string Lch(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal)
    {
        return base.Lch(colorFormat);
    }

    [DataGenerationModuleMethod("Color By Css Space",
        "Generates a color using a specific CSS color space (e.g., srgb, display-p3, rec2020). Example: 'color(rec2020 0.01 0.67 0.27)'")]
    public override string ColorByCssSpace(ColorFormatEnum colorFormat = ColorFormatEnum.Decimal,
        CssSpaceEnum colorSpace = CssSpaceEnum.Srgb)
    {
        return base.ColorByCssSpace(colorFormat, colorSpace);
    }
}