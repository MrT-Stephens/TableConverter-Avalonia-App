using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class WordBase
{
    public abstract ImmutableArray<string> Adjective { get; }
    
    public abstract ImmutableArray<string> Adverb { get; }
    
    public abstract ImmutableArray<string> Conjunction { get; }
    
    public abstract ImmutableArray<string> Interjection { get; }
    
    public abstract ImmutableArray<string> Noun { get; }
    
    public abstract ImmutableArray<string> Preposition { get; }
    
    public abstract ImmutableArray<string> Verb { get; }
}