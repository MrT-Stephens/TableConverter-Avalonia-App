using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class ColorBase
{
    public abstract ImmutableArray<string> HumanName { get; }

    public virtual ImmutableArray<string> Space { get; } =
    [
        "Academy Color Encoding System (ACES)",
        "Adobe RGB",
        "Adobe Wide Gamut RGB",
        "British Standard Colour (BS)",
        "CIE 1931 XYZ",
        "CIELAB",
        "CIELUV",
        "CIEUVW",
        "CMY",
        "CMYK",
        "DCI-P3",
        "Display-P3",
        "Federal Standard 595C",
        "HKS",
        "HSL",
        "HSLA",
        "HSLuv",
        "HSV",
        "HWB",
        "LCh",
        "LMS",
        "Munsell Color System",
        "Natural Color System (NSC)",
        "Pantone Matching System (PMS)",
        "ProPhoto RGB Color Space",
        "RAL",
        "RG",
        "RGBA",
        "RGK",
        "Rec. 2020",
        "Rec. 2100",
        "Rec. 601",
        "Rec. 709",
        "Uniform Color Spaces (UCSs)",
        "YDbDr",
        "YIQ",
        "YPbPr",
        "sRGB",
        "sYCC",
        "scRGB",
        "xvYCC",
    ];
}