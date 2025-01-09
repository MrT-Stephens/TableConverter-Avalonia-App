using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Phone",
    "Module for generating phone-related data such as phone numbers and IMEI numbers.",
    "DataGenerationPhoneIcon")]
public class PhoneAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : PhoneModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Phone Number",
        "Generates a phone number based on the specified type. Example: '(555) 123-4567'")]
    public override string PhoneNumber(PhoneNumberType type = PhoneNumberType.Human)
    {
        return base.PhoneNumber(type);
    }

    [DataGenerationModuleMethod("Imei",
        "Generates a random International Mobile Equipment Identity (IMEI) number. Example: '356938035643809'")]
    public override string Imei()
    {
        return base.Imei();
    }
}