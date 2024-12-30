using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Science",
    "Module for generating science-related data such as chemical elements and measurement units.",
    "DataGenerationScienceIcon")]
public class ScienceAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ScienceModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Chemical Element",
        "Generates a random chemical element name (e.g., 'Hydrogen', 'Oxygen').")]
    public override string ChemicalElement()
    {
        return base.ChemicalElement();
    }

    [DataGenerationModuleMethod("Unit", "Generates a random unit of measurement (e.g., 'meter', 'kilogram').")]
    public override string Unit()
    {
        return base.Unit();
    }
}