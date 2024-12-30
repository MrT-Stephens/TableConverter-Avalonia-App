using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum PhoneNumberType
{
    National,
    International,
    Human
}

public class PhoneModule(FakerBase faker, LocaleBase locale, Randomizer randomizer) : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Phone";

    public virtual string PhoneNumber(PhoneNumberType type = PhoneNumberType.Human)
    {
        return type switch
        {
            PhoneNumberType.Human => Randomizer.ReplaceSymbolsWithNumbers(
                Randomizer.GetWeightedValue(Locale.PhoneNumber.Value.HumanPattern)),
            PhoneNumberType.International => Randomizer.ReplaceSymbolsWithNumbers(
                Randomizer.GetWeightedValue(Locale.PhoneNumber.Value.InternationalPattern)),
            PhoneNumberType.National => Randomizer.ReplaceSymbolsWithNumbers(
                Randomizer.GetWeightedValue(Locale.PhoneNumber.Value.NationalPattern)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public virtual string Imei()
    {
        return Randomizer.ReplaceCreditCardSymbols("##-######-######-L");
    }
}