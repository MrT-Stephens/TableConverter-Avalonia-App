using System.Globalization;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class FoodModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Food";

    public virtual string Adjective()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Adjective);
    }

    public virtual string Description()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.DescriptionPattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string Dish()
    {
        return Randomizer.Bool()
            ? ToTitleCase(Randomizer.GetRandomElement(Locale.Food.Value.Dish))
            : ToTitleCase(Randomizer.GetRandomElement(Locale.Food.Value.DishPattern).Build(Faker, Locale, Randomizer));
    }
    
    public virtual string EthnicCategory()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.EthnicCategory);
    }
    
    public virtual string Fruit()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Fruit);
    }
    
    public virtual string Ingredient()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Ingredient);
    }
    
    public virtual string Meat()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Meat);
    }
    
    public virtual string Spice()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Spice);
    }
    
    public virtual string Vegetable()
    {
        return Randomizer.GetRandomElement(Locale.Food.Value.Vegetable);
    }

    private static string ToTitleCase(string input)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input);
    }
}