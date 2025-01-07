using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Food",
    "Module for generating food-related data such as dish names, ingredients, and ethnic categories.",
    "DataGenerationFoodIcon")]
public class FoodAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : FoodModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Adjective",
        "Generates a random adjective used to describe a dish. Example: 'crispy'")]
    public override string Adjective()
    {
        return base.Adjective();
    }

    [DataGenerationModuleMethod("Description",
        "Generates a random dish description, such as a detailed and flavorful description of a meal. Example: 'An exquisite ostrich roast, infused with the essence of longan, slow-roasted to bring out its natural flavors and served with a side of creamy red cabbage.'")]
    public override string Description()
    {
        return base.Description();
    }

    [DataGenerationModuleMethod("Ingredient",
        "Generates a random ingredient used in a dish. Example: 'butter'")]
    public override string Ingredient()
    {
        return base.Ingredient();
    }

    [DataGenerationModuleMethod("Spice",
        "Generates a random spice name used in cooking. Example: 'chilli'")]
    public override string Spice()
    {
        return base.Spice();
    }

    [DataGenerationModuleMethod("Vegetable",
        "Generates a random vegetable name that can be used in dishes. Example: 'broccoli'")]
    public override string Vegetable()
    {
        return base.Vegetable();
    }

    [DataGenerationModuleMethod("Fruit",
        "Generates a random fruit name that can be used in dishes. Example: 'lemon'")]
    public override string Fruit()
    {
        return base.Fruit();
    }

    [DataGenerationModuleMethod("Meat",
        "Generates a random meat name that can be used in dishes. Example: 'venison'")]
    public override string Meat()
    {
        return base.Meat();
    }

    [DataGenerationModuleMethod("Dish",
        "Generates a random dish name. Example: 'Tagine-Rubbed Venison Salad'")]
    public override string Dish()
    {
        return base.Dish();
    }

    [DataGenerationModuleMethod("Ethnic Category",
        "Generates a random ethnic category for a dish, such as Italian, Chinese, etc. Example: 'Italian'")]
    public override string EthnicCategory()
    {
        return base.EthnicCategory();
    }
}