using System.Reflection;
using TableConverter.DataGeneration.Exceptions;

namespace TableConverter.DataGeneration.Tests.FactoryTests;

public class LocaleFactoryTest
{
    [Theory]
    [InlineData("en")]
    [InlineData("en_GB")]
    [InlineData("zh_CN")]
    public void CreateLocale_Returns_A_Locale_For_Known_Identifiers(string localeType)
    {
        var locale = LocaleFactory.CreateLocale(localeType);

        Assert.NotNull(locale);
    }

    [Fact]
    public void CreateLocale_Returns_A_Type_Marked_With_The_Requested_Locale()
    {
        var locale = LocaleFactory.CreateLocale("en_GB");

        var attribute = Assert.IsType<LocaleAttribute>(
            Attribute.GetCustomAttribute(locale.GetType(), typeof(LocaleAttribute)));

        Assert.Equal("en_GB", attribute.Locale);
    }

    [Fact]
    public void CreateLocale_Returns_A_New_Instance_On_Each_Call()
    {
        // Locales hold mutable dataset state, so callers must not receive a shared instance.
        Assert.NotSame(LocaleFactory.CreateLocale("en"), LocaleFactory.CreateLocale("en"));
    }

    [Fact]
    public void CreateLocale_Throws_A_LocaleNotFoundException_For_Unknown_Identifiers()
    {
        var exception = Assert.Throws<LocaleNotFoundException>(() => LocaleFactory.CreateLocale("xx_XX"));

        Assert.Equal("xx_XX", exception.LocaleType);
        Assert.Contains("xx_XX", exception.Message);
    }

    [Fact]
    public void CreateLocale_Does_Not_Throw_The_Bare_Base_Exception()
    {
        var exception = Record.Exception(() => LocaleFactory.CreateLocale("not_a_locale"));

        Assert.NotNull(exception);
        Assert.IsType<LocaleNotFoundException>(exception);
        Assert.NotEqual(typeof(Exception), exception.GetType());
    }

    [Fact]
    public void LoadLocaleNames_Returns_Every_Known_Locale_Exactly_Once()
    {
        var names = LocaleFactory.LoadLocaleNames();

        Assert.Contains("en", names);
        Assert.Contains("en_GB", names);
        Assert.Contains("zh_CN", names);
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void LoadLocaleNames_Returns_The_Cached_Catalog()
    {
        // The catalog is built from assembly reflection once and then reused.
        Assert.Same(LocaleFactory.LoadLocaleNames(), LocaleFactory.LoadLocaleNames());
    }

    [Fact]
    public void LoadLocaleNames_Result_Cannot_Be_Mutated_By_Callers()
    {
        var names = LocaleFactory.LoadLocaleNames();

        // An array or List would let a caller mutate the cached catalog through a cast.
        Assert.False(names is string[]);
        Assert.False(names is List<string>);
    }

    [Fact]
    public void Faker_Throws_A_LocaleNotFoundException_For_An_Unknown_Locale()
    {
        var faker = new Faker();

        var exception = Assert.Throws<LocaleNotFoundException>(() => faker.LocaleType = "xx_XX");

        Assert.Equal("xx_XX", exception.LocaleType);
    }

    [Fact]
    public void Faker_Remains_Usable_After_An_Unknown_Locale_Is_Rejected()
    {
        var faker = new Faker();
        var originalLocaleType = faker.LocaleType;

        Assert.Throws<LocaleNotFoundException>(() => faker.LocaleType = "xx_XX");

        // The rejected identifier must not be adopted, and generation must keep working.
        Assert.Equal(originalLocaleType, faker.LocaleType);
        Assert.False(string.IsNullOrWhiteSpace(faker.Person.FullName()));
    }

    [Fact]
    public void All_Locale_Names_Are_Resolvable()
    {
        foreach (var name in LocaleFactory.LoadLocaleNames())
        {
            var locale = LocaleFactory.CreateLocale(name);

            Assert.NotNull(locale);
        }
    }

    [Fact]
    public void LocaleNotFoundException_Exposes_The_Locale_And_Overrides_ToString()
    {
        var exception = new LocaleNotFoundException("zz", "boom");

        Assert.Equal("zz", exception.LocaleType);
        Assert.Equal("boom", exception.Message);
        Assert.Contains("zz", exception.ToString());
    }

    [Fact]
    public void LocaleFactory_Does_Not_Expose_Mutable_Static_State()
    {
        // Guards against re-introducing an uncached, per-call reflection scan.
        var fields = typeof(LocaleFactory).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.All(fields, field => Assert.True(field.IsInitOnly || field.IsLiteral,
            $"Static field '{field.Name}' should be readonly."));
    }
}
