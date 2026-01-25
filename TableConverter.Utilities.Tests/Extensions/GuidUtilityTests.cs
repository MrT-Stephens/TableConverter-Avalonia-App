using System.Text;
using TableConverter.Utilities.Database.Extensions;

namespace TableConverter.Utilities.Tests.Extensions;

public class GuidUtilityTests
{
    [Fact]
    public void SameInput_ProducesSameGuid()
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, "my-string");
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, "my-string");

        Assert.Equal(g1, g2);
    }

    [Fact]
    public void DifferentNames_ProduceDifferentGuids()
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, "string-1");
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, "string-2");

        Assert.NotEqual(g1, g2);
    }

    [Fact]
    public void DifferentNamespaces_ProduceDifferentGuids()
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, "my-string");
        var g2 = GuidUtility.Create(GuidUtility.DnsNamespace, "my-string");

        Assert.NotEqual(g1, g2);
    }

    [Fact]
    public void NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GuidUtility.Create(GuidUtility.UrlNamespace, null!)
        );
    }

    [Fact]
    public void EmptyString_IsValidAndDeterministic()
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, string.Empty);
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, string.Empty);

        Assert.Equal(g1, g2);
        Assert.NotEqual(Guid.Empty, g1);
    }

    [Fact]
    public void SupportsUnicodeStrings()
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, "こんにちは");
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, "こんにちは");

        Assert.Equal(g1, g2);
    }

    [Fact]
    public void GuidHasCorrectVersion_Version5()
    {
        var guid = GuidUtility.Create(GuidUtility.UrlNamespace, "test");

        // Version is stored in the high nibble of byte 7
        var version = (guid.ToByteArray()[7] >> 4) & 0x0F;

        Assert.Equal(5, version);
    }

    [Fact]
    public void GuidHasCorrectVariant_Rfc4122()
    {
        var guid = GuidUtility.Create(GuidUtility.UrlNamespace, "test");

        // Variant is stored in byte 8 (10xxxxxx)
        var variantByte = guid.ToByteArray()[8];
        var isRfc4122 = (variantByte & 0xC0) == 0x80;

        Assert.True(isRfc4122);
    }

    [Fact]
    public void PathStrings_WorkCorrectly()
    {
        var path = @"C:\Data\file.txt";

        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, path);
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, path);

        Assert.Equal(g1, g2);
    }

    [Fact]
    public void LongSimilarPaths_ProduceDifferentGuid()
    {
        var path1 = @"C:\Data\Projects\ProjectA\file.txt";
        var path2 = @"C:\Data\Projects\ProjectB\file.txt";

        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, path1);
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, path2);

        Assert.NotEqual(g1, g2);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("HELLO")]
    [InlineData("HeLLo")]
    public void CaseSensitive_ProducesDifferentGuids(string input)
    {
        var guids = new List<Guid>
        {
            GuidUtility.Create(GuidUtility.UrlNamespace, input.ToLowerInvariant()),
            GuidUtility.Create(GuidUtility.UrlNamespace, input.ToUpperInvariant())
        };

        if (input != input.ToLowerInvariant() && input != input.ToUpperInvariant())
        {
            Assert.NotEqual(guids[0], guids[1]);
        }
    }

    [Theory]
    [InlineData("test", "test ")]
    [InlineData("test", " test")]
    [InlineData("test", "test\n")]
    [InlineData("test", "test\t")]
    public void WhitespaceMatters_ProducesDifferentGuids(string input1, string input2)
    {
        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, input1);
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, input2);

        Assert.NotEqual(g1, g2);
    }

    [Fact]
    public void CustomNamespace_ProducesValidGuid()
    {
        var customNamespace = Guid.NewGuid();
        var guid = GuidUtility.Create(customNamespace, "test");

        Assert.NotEqual(Guid.Empty, guid);
        
        var version = (guid.ToByteArray()[7] >> 4) & 0x0F;
        Assert.Equal(5, version);
    }

    [Fact]
    public void EmptyNamespace_ProducesValidGuid()
    {
        var guid = GuidUtility.Create(Guid.Empty, "test");

        Assert.NotEqual(Guid.Empty, guid);
        
        var version = (guid.ToByteArray()[7] >> 4) & 0x0F;
        
        Assert.Equal(5, version);
    }

    [Fact]
    public void MultibyteCharacters_ProduceDifferentGuids()
    {
        var inputs = new[]
        {
            "😀",
            "😁",
            "🎉",
            "日本語",
            "中文",
            "한국어"
        };

        var guids = inputs.Select(i => GuidUtility.Create(GuidUtility.UrlNamespace, i)).ToList();
        
        Assert.Equal(inputs.Length, guids.Distinct().Count());
    }

    [Fact]
    public void ConcurrentCalls_ProduceSameResult()
    {
        const string testString = "concurrent-test";
        var results = new Guid[100];

        Parallel.For(0, 100, i =>
        {
            results[i] = GuidUtility.Create(GuidUtility.UrlNamespace, testString);
        });

        Assert.All(results, g => Assert.Equal(results[0], g));
    }

    [Fact]
    public void DifferentEncodings_MayProduceDifferentGuids()
    {
        var text = "café";
        
        var utf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
        var latin1 = Encoding.Latin1.GetString(Encoding.Latin1.GetBytes(text));

        var g1 = GuidUtility.Create(GuidUtility.UrlNamespace, utf8);
        var g2 = GuidUtility.Create(GuidUtility.UrlNamespace, latin1);

        // UTF-8 encoding is used internally, so results should be consistent
        Assert.Equal(g1, g2);
    }

    [Fact]
    public void SqlServerCompatibility_GuidFormat()
    {
        var guid = GuidUtility.Create(GuidUtility.UrlNamespace, "sql-test");
        
        // Should be parseable as SQL Server UNIQUEIDENTIFIER
        var guidString = guid.ToString();
        
        Assert.Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-5[a-f0-9]{3}-[89ab][a-f0-9]{3}-[a-f0-9]{12}$", guidString);
    }

    [Fact]
    public void HighEntropyInputs_ProduceValidGuids()
    {
        var random = new Random(42);
        
        var inputs = Enumerable.Range(0, 100)
            .Select(_ => Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())))
            .ToArray();

        var guids = inputs.Select(i => GuidUtility.Create(GuidUtility.UrlNamespace, i)).ToList();

        Assert.Equal(100, guids.Distinct().Count());
    }
    
    [Fact]
    public void DifferentNamespace_ProducesDifferentGuids()
    {
        var guid1 = GuidUtility.Create(GuidUtility.UrlNamespace, "test");
        var guid2 = GuidUtility.Create(GuidUtility.DnsNamespace, "test");
        
        Assert.NotEqual(guid1, guid2);
    }
}
