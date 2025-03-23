using System.Reflection;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.Tests.TestBase;

/// <summary>
///     This abstract base class provides common functionality for testing input converter handlers.
///     It expects an implementation of IConverterHandlerInput (generic type TInputConverter) and a corresponding test data
///     class (TInputConverterData)
///     that provides test cases. The class facilitates both synchronous and asynchronous file conversion tests, including
///     the necessary assertions to validate the conversion process.
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
    ///     Synchronous test method that tests file conversion functionality.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetSuccessfulTestCases))]
    public void TestInputFile_WithSuccessfulData(
        string fileName,
        ConverterHandlerBaseOptions options,
        TableData expectedTableDataResult)
    {
        Handler.Options = options; // Sets the options for the handler.

        using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        // Perform file reading and conversion, asserting success at each stage.
        var fileResult = Handler.ReadFile(stream);
        Assert.True(fileResult.IsSuccess, $"fileResult.IsSuccess is false. Error: {fileResult.Error}");

        var convertResult = Handler.ReadText(fileResult.Value);
        Assert.True(convertResult.IsSuccess, $"convertResult.IsSuccess is false. Error: {convertResult.Error}");

        // Compare the actual conversion result with the expected data.
        Assert.Equal(expectedTableDataResult, convertResult.Value);
    }

    /// <summary>
    ///     Asynchronous test method that tests asynchronous file conversion functionality.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetSuccessfulTestCases))]
    public async Task TestInputFileAsync_WithSuccessfulData(
        string fileName,
        ConverterHandlerBaseOptions options,
        TableData expectedTableDataResult)
    {
        Handler.Options = options; // Sets the options for the handler.

        await using var stream = GetFileStream(fileName); // Asynchronously retrieves the file stream.

        // Perform asynchronous file reading and conversion, asserting success at each stage.
        var fileResult = await Handler.ReadFileAsync(stream);
        Assert.True(fileResult.IsSuccess, $"fileResult.IsSuccess is false. Error: {fileResult.Error}");

        var convertResult = await Handler.ReadTextAsync(fileResult.Value);
        Assert.True(convertResult.IsSuccess, $"convertResult.IsSuccess is false. Error: {convertResult.Error}");

        // Compare the actual asynchronous conversion result with the expected data.
        Assert.Equal(expectedTableDataResult, convertResult.Value);
    }

    /// <summary>
    ///     Synchronous test method that tests file conversion functionality with incorrect data.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetFailTestCases))]
    public virtual void TestInputFile_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options; // Sets the options for the handler.

        using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        // Perform file reading and conversion, asserting success at each stage.
        var fileResult = Handler.ReadFile(stream);
        Assert.True(fileResult.IsSuccess, $"fileResult.IsSuccess is false. Error: {fileResult.Error}");

        var convertResult = Handler.ReadText(fileResult.Value);
        Assert.False(convertResult.IsSuccess,
            $"convertResult.IsSuccess is true. Should be false due to data being incorrect. Data: {convertResult.Value}");
    }

    /// <summary>
    ///     Asynchronous test method that tests asynchronous file conversion functionality with incorrect data.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetFailTestCases))]
    public virtual async Task TestInputFileAsync_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options; // Sets the options for the handler.

        await using var stream = GetFileStream(fileName); // Asynchronously retrieves the file stream.

        // Perform asynchronous file reading and conversion, asserting success at each stage.
        var fileResult = await Handler.ReadFileAsync(stream);
        Assert.True(fileResult.IsSuccess, $"fileResult.IsSuccess is false. Error: {fileResult.Error}");

        var convertResult = await Handler.ReadTextAsync(fileResult.Value);
        Assert.False(convertResult.IsSuccess,
            $"convertResult.IsSuccess is true. Should be false due to data being incorrect. Data: {convertResult.Value}");
    }
    
    /// <summary>
    ///     Test method that tests file conversion functionality with empty data.
    /// </summary>
    /// <param name="fileName"></param>
    [Theory]
    [InlineData("test_input_empty.txt")]
    [InlineData("test_input_whitespace.txt")]
    public virtual void TestInputFile_WithEmptyData(string fileName)
    {
        using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        // Perform file reading, asserting failure due to empty data.
        var fileResult = Handler.ReadFile(stream);
        Assert.False(fileResult.IsSuccess, $"fileResult.IsSuccess is true. Should be false due to empty data.");
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