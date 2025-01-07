using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Vehicle",
    "Module for generating vehicle-related data such as manufacturer, model, type, and more.",
    "DataGenerationVehicleIcon")]
public class VehicleAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : VehicleModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Vehicle",
        "Generates a random vehicle description, typically combining manufacturer and model. Example: 'Toyota Corolla'")]
    public override string Vehicle()
    {
        return base.Vehicle();
    }

    [DataGenerationModuleMethod("Manufacturer",
        "Generates the name of a vehicle manufacturer. Example: 'Toyota', 'Ford', or 'Tesla'")]
    public override string Manufacturer()
    {
        return base.Manufacturer();
    }

    [DataGenerationModuleMethod("Model",
        "Generates the model name of a vehicle. Example: 'Corolla', 'Mustang', or 'Model S'")]
    public override string Model()
    {
        return base.Model();
    }

    [DataGenerationModuleMethod("Type",
        "Generates the type of vehicle, such as sedan, truck, or SUV. Example: 'SUV' or 'Hatchback'")]
    public override string Type()
    {
        return base.Type();
    }

    [DataGenerationModuleMethod("Fuel",
        "Generates the type of fuel used by the vehicle, such as gasoline, diesel, or electric. Example: 'Electric'")]
    public override string Fuel()
    {
        return base.Fuel();
    }

    [DataGenerationModuleMethod("Vin",
        "Generates a random Vehicle Identification Number (VIN). Example: '1HGCM82633A123456'")]
    public override string Vin()
    {
        return base.Vin();
    }

    [DataGenerationModuleMethod("Color",
        "Generates a random color for the vehicle. Example: 'Red', 'Blue', or 'Black'")]
    public override string Color()
    {
        return base.Color();
    }

    [DataGenerationModuleMethod("Vrm",
        "Generates a random Vehicle Registration Mark (VRM), also known as a license plate number. Example: 'AB12CDE'")]
    public override string Vrm()
    {
        return base.Vrm();
    }

    [DataGenerationModuleMethod("Bicycle",
        "Generates a random bicycle. Example: 'Tricycle'")]
    public override string Bicycle()
    {
        return base.Bicycle();
    }
}