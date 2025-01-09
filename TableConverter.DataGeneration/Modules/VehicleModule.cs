using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class VehicleModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Vehicle";

    public virtual string Vehicle()
    {
        return $"{Manufacturer()} {Model()}";
    }

    public virtual string Manufacturer()
    {
        return Randomizer.GetRandomElement(Locale.Vehicle.Value.Manufacturer);
    }

    public virtual string Model()
    {
        return Randomizer.GetRandomElement(Locale.Vehicle.Value.Model);
    }

    public virtual string Type()
    {
        return Randomizer.GetRandomElement(Locale.Vehicle.Value.VehicleType);
    }

    public virtual string Fuel()
    {
        return Randomizer.GetRandomElement(Locale.Vehicle.Value.FuelType);
    }

    public virtual string Bicycle()
    {
        return Randomizer.GetRandomElement(Locale.Vehicle.Value.BicycleType);
    }

    public virtual string Vin()
    {
        return Randomizer.ReplaceSymbols("••••••••••?•#####");
    }

    public virtual string Vrm()
    {
        return Randomizer.ReplaceSymbols("??##???");
    }

    public virtual string Color()
    {
        return Faker.Color.Human();
    }
}