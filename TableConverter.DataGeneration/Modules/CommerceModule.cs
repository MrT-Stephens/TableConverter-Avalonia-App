using TableConverter.DataGeneration.Exceptions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum IsbnLengthEnum
{
    Isbn10,
    Isbn13
}

public class CommerceModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Commerce";

    public virtual string Department()
    {
        return Randomizer.GetRandomElement(Locale.Commerce.Value.Department);
    }

    public virtual string ProductName()
    {
        return $"{ProductAdjective()} {ProductMaterial()} {Product()}";
    }

    public virtual string Price(decimal minNumber = 0, decimal maxNumber = 1000, int decimalPlaces = 2,
        string symbol = "")
    {
        return $"{symbol}{Faker.Number.Decimal(minNumber, maxNumber, decimalPlaces)}";
    }

    public virtual string ProductAdjective()
    {
        return Randomizer.GetRandomElement(Locale.Commerce.Value.ProductName.Adjective);
    }

    public virtual string ProductMaterial()
    {
        return Randomizer.GetRandomElement(Locale.Commerce.Value.ProductName.Material);
    }

    public virtual string Product()
    {
        return Randomizer.GetRandomElement(Locale.Commerce.Value.ProductName.Product);
    }

    public virtual string ProductDescription()
    {
        return Randomizer.GetRandomElement(Locale.Commerce.Value.ProductDescription).Build(Faker, Locale, Randomizer);
    }

    public virtual string Isbn(IsbnLengthEnum length = IsbnLengthEnum.Isbn13, string separator = "")
    {
        return length switch
        {
            IsbnLengthEnum.Isbn10 => GenerateIsbn10(separator),
            IsbnLengthEnum.Isbn13 => GenerateIsbn13(separator),
            _ => FakerArgumentException.CreateException<string>(length, "Invalid ISBN length")
        };
    }

    private string GenerateIsbn13(string separator)
    {
        // ISBN-13 must start with 978 or 979
        var prefix = Randomizer.Bool() ? "978" : "979";

        // Generate a random number for the next digits
        var randomDigits = Randomizer.Number(100000000, 999999999);
        var isbn = prefix + randomDigits;

        // Add checksum digit
        var checksum = CalculateIsbn13Checksum(isbn);

        // Format the ISBN with the separator
        return FormatIsbnWithSeparator(isbn + checksum, separator);
    }

    private static int CalculateIsbn13Checksum(string isbn)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++) sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);

        var remainder = sum % 10;
        var checksum = (10 - remainder) % 10;
        return checksum;
    }

    private string GenerateIsbn10(string separator)
    {
        // Generate the first 9 digits randomly
        var randomDigits = Randomizer.Number(100000000, 999999999); // 9 digits
        var isbn = randomDigits.ToString();

        // Calculate the checksum
        var checksum = CalculateIsbn10Checksum(isbn);

        // Combine the digits with the checksum
        isbn += checksum;

        // Format the ISBN-10 with the separator
        return FormatIsbnWithSeparator(isbn, separator);
    }

    private static char CalculateIsbn10Checksum(string isbn)
    {
        var sum = 0;
        for (var i = 0; i < 9; i++) sum += (isbn[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var checksumValue = 11 - remainder;

        // Handle edge cases:
        // If checksum is 10, return 'X'
        // If checksum is 11, return '0'
        return checksumValue switch
        {
            10 => 'X',
            11 => '0',
            _ => (char)(checksumValue + '0')
        };
    }

    private static string FormatIsbnWithSeparator(string isbn, string separator)
    {
        return isbn.Length switch
        {
            13 => string.Join(separator, isbn.Substring(0, 3), isbn.Substring(3, 1), isbn.Substring(4, 4),
                isbn.Substring(8, 4), isbn.Substring(12, 1)),
            10 => string.Join(separator, isbn.Substring(0, 1), isbn.Substring(1, 3), isbn.Substring(4, 3),
                isbn.Substring(7, 3)),
            _ => isbn
        };
    }
}