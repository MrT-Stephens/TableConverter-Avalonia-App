using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Commerce", "Module to generate commerce and product related entries.",
    "DataGenerationCommerceIcon")]
public class CommerceAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : CommerceModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Department", "Returns a random department name, such as 'Garden' or 'Electronics'.")]
    public override string Department()
    {
        return base.Department();
    }

    [DataGenerationModuleMethod("Product Name",
        "Generates a random descriptive product name, such as 'Incredible Soft Gloves'.")]
    public override string ProductName()
    {
        return base.ProductName();
    }

    [DataGenerationModuleMethod("Price",
        "Generates a random price between the given range. Includes support for decimals and custom currency symbols.")]
    public override string Price(decimal minNumber = 0, decimal maxNumber = 1000, int decimalPlaces = 2,
        string symbol = "")
    {
        return base.Price(minNumber, maxNumber, decimalPlaces, symbol);
    }

    [DataGenerationModuleMethod("Product Adjective",
        "Returns an adjective describing a product, such as 'Handcrafted' or 'Ergonomic'.")]
    public override string ProductAdjective()
    {
        return base.ProductAdjective();
    }

    [DataGenerationModuleMethod("Product Material",
        "Returns a material commonly used in products, such as 'Rubber' or 'Steel'.")]
    public override string ProductMaterial()
    {
        return base.ProductMaterial();
    }

    [DataGenerationModuleMethod("Product", "Returns a short product name, such as 'Computer' or 'Chair'.")]
    public override string Product()
    {
        return base.Product();
    }

    [DataGenerationModuleMethod("Product Description",
        "Generates a detailed product description using various components, such as 'Featuring advanced rubber technology for exceptional durability'.")]
    public override string ProductDescription()
    {
        return base.ProductDescription();
    }

    [DataGenerationModuleMethod("Isbn",
        "Generates a random ISBN identifier. Supports both ISBN-10 and ISBN-13 formats with optional separators.")]
    public override string Isbn(IsbnLengthEnum length = IsbnLengthEnum.Isbn13, string separator = "")
    {
        return base.Isbn(length, separator);
    }
}