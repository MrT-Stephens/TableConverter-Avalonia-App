using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Music",
    "Module for generating music-related data, including album names, artist names, song names, and genres.",
    "DataGenerationMusicIcon")]
public class MusicAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : MusicModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Album", "Generates a random album name (e.g., 'Abbey Road', 'Back in Black').")]
    public override string Album()
    {
        return base.Album();
    }

    [DataGenerationModuleMethod("Artist", "Generates a random artist name (e.g., 'The Beatles', 'Adele').")]
    public override string Artist()
    {
        return base.Artist();
    }

    [DataGenerationModuleMethod("Genre", "Generates a random music genre (e.g., 'Rock', 'Pop').")]
    public override string Genre()
    {
        return base.Genre();
    }

    [DataGenerationModuleMethod("Song Name", "Generates a random song name (e.g., 'Yesterday', 'Shape of You').")]
    public override string SongName()
    {
        return base.SongName();
    }
}