using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels;

public abstract class ConverterProvider : IConverterProvider
{
    public abstract IConverterMetadata Metadata { get; }

    public abstract IConverterHandlerInput CreateInputHander();

    public abstract IConverterHandlerOutput CreateOutputHander();

    public IConverterHandlerInput? InputHandler()
    {
        if (!Metadata.Support.HasFlag(ConverterSupport.Input))
        {
            return null;
        }

        return CreateInputHander()
            ?? throw new InvalidOperationException("Converter handler supports input conversion. But failed to create a input converter.");
    }

    public IConverterHandlerOutput? OutputHandler()
    {
        if (!Metadata.Support.HasFlag(ConverterSupport.Output))
        {
            return null;
        }

        return CreateOutputHander()
            ?? throw new InvalidOperationException("Converter handler supports output conversion. But failed to create a output converter.");
    }
}
