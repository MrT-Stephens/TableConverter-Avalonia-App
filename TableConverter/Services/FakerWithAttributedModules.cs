using TableConverter.DataGeneration;
using TableConverter.DataGeneration.Modules;
using TableConverter.Services.DataGenerationAttributedModules;

namespace TableConverter.Services;

public class FakerWithAttributedModules(string localeType = "en", int? seed = null) : FakerBase(localeType, seed)
{
    public override PersonModule Person => new PersonAttributedModule(this, Locale, Randomizer);
    public override PhoneModule Phone => new PhoneAttributedModule(this, Locale, Randomizer);
    public override LocationModule Location => new LocationAttributedModule(this, Locale, Randomizer);
    public override InternetModule Internet => new InternetAttributedModule(this, Locale, Randomizer);
    public override WordModule Word => new WordAttributedModule(this, Locale, Randomizer);
    public override LoremModule Lorem => new LoremAttributedModule(this, Locale, Randomizer);
    public override SystemModule System => new SystemAttributedModule(this, Locale, Randomizer);
    public override ScienceModule Science => new ScienceAttributedModule(this, Locale, Randomizer);
    public override MusicModule Music => new MusicAttributedModule(this, Locale, Randomizer);
}