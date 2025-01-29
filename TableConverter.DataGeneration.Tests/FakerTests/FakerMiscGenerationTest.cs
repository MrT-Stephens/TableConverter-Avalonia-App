namespace TableConverter.DataGeneration.Tests.FakerTests;

public class FakerMiscGenerationTest(Faker faker) : IClassFixture<Faker>
{
    [Theory]
    [InlineData("en")]
    [InlineData("en_GB")]
    [InlineData("zh_CN")]
    public void TestFaker_ShouldBeThreadSafe(string locale)
    {
        try
        {
            faker.LocaleType = locale;

            for (var i = 0; i < 1000; i++)
            {
                Parallel.Invoke(
                    () =>
                    {
                        faker.Person.FullName();
                        faker.Color.Cmyk();
                        faker.Location.City();
                        faker.Internet.Email();
                        faker.Phone.PhoneNumber();
                        faker.Number.Integer(1, 100);
                        faker.Science.ChemicalElement();
                        faker.System.Semver();
                    });
            }
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}