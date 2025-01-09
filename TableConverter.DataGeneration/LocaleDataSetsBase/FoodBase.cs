using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class FoodBase
{
    public abstract ImmutableArray<string> Adjective { get; }

    public abstract ImmutableArray<ITemplatedValueBuilder<FakerBase, LocaleBase>> DescriptionPattern { get; }

    public abstract ImmutableArray<string> Dish { get; }

    public abstract ImmutableArray<ITemplatedValueBuilder<FakerBase, LocaleBase>> DishPattern { get; }

    public abstract ImmutableArray<string> EthnicCategory { get; }

    public abstract ImmutableArray<string> Fruit { get; }

    public abstract ImmutableArray<string> Ingredient { get; }

    public abstract ImmutableArray<string> Meat { get; }

    public abstract ImmutableArray<string> Spice { get; }

    public abstract ImmutableArray<string> Vegetable { get; }
}