using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class MusicBase
{
    public abstract ImmutableArray<string> Album { get; }

    public abstract ImmutableArray<string> Artist { get; }

    public abstract ImmutableArray<string> Genre { get; }

    public abstract ImmutableArray<string> SongName { get; }
}