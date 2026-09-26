using System.Reflection;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.TestBase;

/// <summary>
///     This abstract base class provides common functionality for testing input converter handlers.
///     It expects an implementation of IConverterHandlerInput (generic type TInputConverter) and a corresponding test data
///     class (TInputConverterData)
///     that provides test cases. The class drives the handler's single read method and asserts on the table it
///     gathers.
/// </summary>
/// <typeparam name="TInputConverter">
///     The input converter handler type that implements IConverterHandlerInput and is used
///     for the tests.
/// </typeparam>
/// <typeparam name="TInputConverterData">
///     The type of the class that holds the test cases. It must inherit from
///     InputConverterTestCasesBase.
/// </typeparam>
public abstract class InputConverterTestBase<TInputConverter, TInputConverterData>
    // Ensures that a new instance of TInputConverter is created for each test class.
    : IClassFixture<TInputConverter>
// Constraints TInputConverter to be a class, implement IConverterHandlerInput, and have a parameterless constructor.
    where TInputConverter : class, IConverterHandlerInput, new()
    // Constraints TInputConverterData to extend InputConverterTestCasesBase and be instantiated with a parameterless constructor.
    where TInputConverterData : InputConverterTestCasesBase, new()
{
    protected readonly TInputConverter Handler = new();

    /// <summary>
    ///     Reads a file straight into a sink, which is the only path a read takes, and checks that the
    ///     gathered table matches the expected data.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetSuccessfulTestCases))]
    public async Task TestInputFile_WithSuccessfulData(
        string fileName,
        ConverterHandlerBaseOptions options,
        TableSnapshot expectedTableResult)
    {
        Handler.Options = options; // Sets the options for the handler.

        await using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        var sink = new TableSnapshot();

        var result = await Handler.ReadStreamAsync(stream, sink);

        Assert.True(result.IsSuccess, $"result.IsSuccess is false. Error: {result.Error}");
        Assert.True(sink.IsCompleted, "The sink was not completed, so the table would be discarded.");

        // Compare the gathered table with the expected data.
        Assert.Equal(expectedTableResult, sink);
    }

    /// <summary>
    ///     A parse that fails must leave the sink uncompleted, which is what tells the destination to
    ///     discard whatever was written rather than keep half a table.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetFailTestCases))]
    public virtual async Task TestInputFile_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options; // Sets the options for the handler.

        await using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        var sink = new TableSnapshot();

        var result = await Handler.ReadStreamAsync(stream, sink);

        Assert.False(result.IsSuccess, "result.IsSuccess is true. Should be false due to data being incorrect.");
        Assert.False(sink.IsCompleted,
            "The sink was completed for a failed parse, so a half read table would be kept.");
    }

    /// <summary>
    ///     Test method that tests file conversion functionality with empty data.
    /// </summary>
    /// <param name="fileName"></param>
    [Theory]
    [InlineData("test_input_empty.txt")]
    [InlineData("test_input_whitespace.txt")]
    public virtual async Task TestInputFile_WithEmptyData(string fileName)
    {
        await using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        var sink = new TableSnapshot();

        // Perform file reading, asserting failure due to empty data.
        var result = await Handler.ReadStreamAsync(stream, sink);

        Assert.False(result.IsSuccess, "result.IsSuccess is true. Should be false due to empty data.");
    }

    /// <summary>
    ///     Static method that returns test cases as input for the test methods.
    /// </summary>
    public static IEnumerable<object[]> GetSuccessfulTestCases()
    {
        return new TInputConverterData().GetSuccessfulTestCases();
    }

    /// <summary>
    ///     Static method that returns test cases as input for the test methods.
    /// </summary>
    public static IEnumerable<object[]> GetFailTestCases()
    {
        return new TInputConverterData().GetFailTestCases();
    }

    /// <summary>
    ///     Helper method that retrieves the file stream from embedded resources.
    /// </summary>
    protected static Stream? GetFileStream(string fileName)
    {
        // Loads the file stream based on the provided file name from embedded resources.
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"TableConverter.FileConverters.Tests.TestFiles.{fileName}");
    }
}