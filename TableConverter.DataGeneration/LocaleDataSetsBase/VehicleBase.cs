using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class VehicleBase
{
    public abstract ImmutableArray<string> BicycleType { get; }
    
    public abstract ImmutableArray<string> FuelType { get; }
    
    public abstract ImmutableArray<string> Manufacturer { get; }
    
    public abstract ImmutableArray<string> Model { get; }
    
    public abstract ImmutableArray<string> VehicleType { get; }
}