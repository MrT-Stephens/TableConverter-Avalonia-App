namespace TableConverter.FileConverters.Exceptions;

public class FileConverterException : Exception
{
    #region Properties
    
    public string ConverterName { get; }
    
    #endregion
    
    #region Constructors

    public FileConverterException(string converterName)
    {
        ConverterName = converterName;
    }

    public FileConverterException(string converterName, string message)
        : base(message)
    {
        ConverterName = converterName;
    }

    public FileConverterException(string converterName, string message, Exception inner)
        : base(message, inner)
    {
        ConverterName = converterName;
    }
    
    #endregion
}