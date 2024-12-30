using System.Collections.Immutable;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class ColorDataSet : ColorBase
{
    public override ImmutableArray<string> HumanName { get; } =
    [
        "azure",
        "black",
        "blue",
        "cyan",
        "fuchsia",
        "gold",
        "green",
        "grey",
        "indigo",
        "ivory",
        "lavender",
        "lime",
        "magenta",
        "maroon",
        "mint green",
        "olive",
        "orange",
        "orchid",
        "pink",
        "plum",
        "purple",
        "red",
        "salmon",
        "silver",
        "sky blue",
        "tan",
        "teal",
        "turquoise",
        "violet",
        "white",
        "yellow"
    ];
}