using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class MusicModule(FakerBase faker, LocaleBase locale, Randomizer randomizer) : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Music";

    public virtual string Genre()
    {
        return Randomizer.GetRandomElement(Locale.Music.Value.Genre);
    }
    
    public virtual string Album()
    {
        return Randomizer.GetRandomElement(Locale.Music.Value.Album);
    }
    
    public virtual string Artist()
    {
        return Randomizer.GetRandomElement(Locale.Music.Value.Artist);
    }
    
    public virtual string SongName()
    {
        return Randomizer.GetRandomElement(Locale.Music.Value.SongName);
    }
}