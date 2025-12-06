using System.Collections.Generic;
using TableConverter.Contracts;

namespace TableConverter.Interfaces;

public interface IConverterTypes
{
    /// <summary>
    /// Gets the list of input converter types.
    /// </summary>
    public IReadOnlyList<ConverterType> InputTypes { get; }
    
    /// <summary>
    /// Gets the list of output converter types.
    /// </summary>
    public IReadOnlyList<ConverterType> OutputTypes { get; }
    
    /// <summary>
    /// Gets the input converter type by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the converter type to get.
    /// </param>
    /// <returns>
    /// The converter type with the specified name.
    /// </returns>
    public ConverterType GetInputConverter(string name);
    
    /// <summary>
    /// Gets the output converter type by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the converter type to get.
    /// </param>
    /// <returns>
    /// The converter type with the specified name.
    /// </returns>
    public ConverterType GetOutputConverter(string name);

    /// <summary>
    /// Gets all names of the input converters.
    /// </summary>
    /// <returns>
    /// <see cref="IEnumerable{T}"/> of names.
    /// </returns>
    public IEnumerable<string> GetInputConverterNames();
    
    /// <summary>
    /// Gets all names of the output converters.
    /// </summary>
    /// <returns>
    /// <see cref="IEnumerable{T}"/> of names.
    /// </returns>
    public IEnumerable<string> GetOutputConverterNames();
}