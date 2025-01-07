using System.Security.Cryptography;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;

namespace TableConverter.DataGeneration;

public class Faker(string localeType = "en", int? seed = null) : FakerBase(localeType, seed)
{
    public override PersonModule Person => new(this, Locale, Randomizer);
    
    public override PhoneModule Phone => new(this, Locale, Randomizer);
    
    public override LocationModule Location => new(this, Locale, Randomizer);
    
    public override InternetModule Internet => new(this, Locale, Randomizer);
    
    public override WordModule Word => new(this, Locale, Randomizer);
    
    public override LoremModule Lorem => new(this, Locale, Randomizer);

    public override SystemModule System => new(this, Locale, Randomizer);
    
    public override ScienceModule Science => new(this, Locale, Randomizer);
    
    public override MusicModule Music => new(this, Locale, Randomizer);
    
    public override NumberModule Number => new(this, Locale, Randomizer);
    
    public override ImageModule Image => new(this, Locale, Randomizer);
    
    public override ColorModule Color => new(this, Locale, Randomizer);
    
    public override VehicleModule Vehicle => new(this, Locale, Randomizer);
    
    public override CompanyModule Company => new(this, Locale, Randomizer);
    
    public override CommerceModule Commerce => new(this, Locale, Randomizer);
    
    public override FoodModule Food => new(this, Locale, Randomizer);
    
    public override DateTimeModule DateTime => new(this, Locale, Randomizer);

    public static FakerBuilderBase<Faker> Create(Faker? faker = null)
    {
        faker ??= new Faker();
        
        return new Builder(faker);
    }

    private sealed class Builder(Faker faker) : FakerBuilderBase<Faker>(faker);
}