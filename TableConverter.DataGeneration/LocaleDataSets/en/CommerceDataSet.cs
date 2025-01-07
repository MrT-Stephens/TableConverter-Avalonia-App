using System.Collections.Immutable;
using System.Globalization;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class CommerceDataSet : CommerceBase
{
    public override ImmutableArray<string> Department { get; } =
    [
        "Automotive",
        "Baby",
        "Beauty",
        "Books",
        "Clothing",
        "Computers",
        "Electronics",
        "Games",
        "Garden",
        "Grocery",
        "Health",
        "Home",
        "Industrial",
        "Jewelry",
        "Kids",
        "Movies",
        "Music",
        "Outdoors",
        "Shoes",
        "Sports",
        "Tools",
        "Toys"
    ];

    public override CommerceProductNameDefinition ProductName { get; } = new(
        [
            "Awesome",
            "Bespoke",
            "Electronic",
            "Elegant",
            "Ergonomic",
            "Fantastic",
            "Generic",
            "Gorgeous",
            "Handcrafted",
            "Handmade",
            "Incredible",
            "Intelligent",
            "Licensed",
            "Luxurious",
            "Modern",
            "Oriental",
            "Practical",
            "Recycled",
            "Refined",
            "Rustic",
            "Sleek",
            "Small",
            "Tasty",
            "Unbranded"
        ],
        [
            "Bronze",
            "Concrete",
            "Cotton",
            "Fresh",
            "Frozen",
            "Granite",
            "Metal",
            "Plastic",
            "Rubber",
            "Soft",
            "Steel",
            "Wooden"
        ],
        [
            "Bacon",
            "Ball",
            "Bike",
            "Car",
            "Chair",
            "Cheese",
            "Chicken",
            "Chips",
            "Computer",
            "Fish",
            "Gloves",
            "Hat",
            "Keyboard",
            "Mouse",
            "Pants",
            "Pizza",
            "Salad",
            "Sausages",
            "Shirt",
            "Shoes",
            "Soap",
            "Table",
            "Towels",
            "Tuna"
        ]
    );

    public override ImmutableArray<ITemplatedValueBuilder<FakerBase, LocaleBase>> ProductDescription { get; } =
    [
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Discover the {Adjective} new {Product} with an exciting mix of {ProductMaterial} ingredients")
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("ProductMaterial", dataset => dataset.Commerce.Value.ProductName.Material),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Ergonomic {Product} made with {ProductMaterial} for all-day {Adjective} support")
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("ProductMaterial", dataset => dataset.Commerce.Value.ProductName.Material)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "Experience the {ColorHuman} brilliance of our {Product}, perfect for {Adjective} environments")
            .AddRandomPlaceholder("ColorHuman", dataset => dataset.Color.Value.HumanName)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "Featuring {ChemicalElement}-enhanced technology, our {Product} offers unparalleled {Adjective} performance")
            .AddPlaceholder("ChemicalElement", (faker, _, _) => faker.Science.ChemicalElement())
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Innovative {Product} featuring {Adjective} technology and {ProductMaterial} construction")
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective)
            .AddRandomPlaceholder("ProductMaterial", dataset => dataset.Commerce.Value.ProductName.Material),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "Introducing the {Country}-inspired {Product}, blending {Adjective} style with local craftsmanship")
            .AddRandomPlaceholder("Country", dataset => dataset.Location.Value.Country)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("New {ColorHuman} {Product} with ergonomic design for {Adjective} comfort")
            .AddRandomPlaceholder("ColorHuman", dataset => dataset.Color.Value.HumanName)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("New {Product} model with {Int1} GB RAM, {Int2} GB storage, and {Adjective} features")
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddPlaceholder("Int1",
                (_, _, randomizer) => randomizer.Number(0, 100).ToString(CultureInfo.InvariantCulture))
            .AddPlaceholder("Int2",
                (_, _, randomizer) => randomizer.Number(0, 1000).ToString(CultureInfo.InvariantCulture))
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Professional-grade {Product} perfect for {Adjective} training and recreational use")
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Stylish {Product} designed to make you stand out with {Adjective} looks")
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "The sleek and {Adjective} {Product} comes with {ColorHuman} LED lighting for smart functionality")
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("ColorHuman", dataset => dataset.Color.Value.HumanName),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "The {ColorHuman} {Product} combines {Country} aesthetics with {ChemicalElement}-based durability")
            .AddRandomPlaceholder("ColorHuman", dataset => dataset.Color.Value.HumanName)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Country", dataset => dataset.Location.Value.Country)
            .AddPlaceholder("ChemicalElement", (faker, _, _) => faker.Science.ChemicalElement()),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("The {CatchPhrase} {Product} offers reliable performance and {Adjective} design")
            .AddPlaceholder("CatchPhrase", (faker, _, _) => faker.Company.CatchPhrase())
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate(
                "The {FirstName} {Product} is the latest in a series of {Adjective} products from {CompanyName}")
            .AddRandomPlaceholder("FirstName", dataset => dataset.Person.Value.FirstName.Generic)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective)
            .AddPlaceholder("CompanyName", (faker, _, _) => faker.Company.Name()),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("{ProductAdjective} {Product} designed with {ProductMaterial} for {Adjective} performance")
            .AddRandomPlaceholder("ProductAdjective", dataset => dataset.Commerce.Value.ProductName.Adjective)
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("ProductMaterial", dataset => dataset.Commerce.Value.ProductName.Material)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("{CompanyName}'s most advanced {Product} technology increases {Adjective} capabilities")
            .AddPlaceholder("CompanyName", (faker, _, _) => faker.Company.Name())
            .AddRandomPlaceholder("Product", dataset => dataset.Commerce.Value.ProductName.Product)
            .AddRandomPlaceholder("Adjective", dataset => dataset.Word.Value.Adjective)
    ];
}