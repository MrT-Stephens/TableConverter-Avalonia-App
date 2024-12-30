using System.Globalization;
using System.Numerics;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class NumberModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Number";

    public virtual string Integer(int minNumber = int.MinValue, int maxNumber = int.MaxValue)
    {
        return Randomizer.Number(minNumber, maxNumber).ToString(CultureInfo.InvariantCulture);
    }

    public virtual string Long(long minNumber = long.MinValue, long maxNumber = long.MaxValue)
    {
        return Randomizer.Long(minNumber, maxNumber).ToString(CultureInfo.InvariantCulture);
    }

    public virtual string BigInteger(string minNumber = "0", string maxNumber = "9999999999999999999999999999")
    {
        if (!System.Numerics.BigInteger.TryParse(minNumber, out var min))
            throw new ArgumentException("minNumber must be a valid BigInteger.", nameof(minNumber));

        if (!System.Numerics.BigInteger.TryParse(maxNumber, out var max))
            throw new ArgumentException("maxNumber must be a valid BigInteger.", nameof(maxNumber));

        if (min > max) throw new ArgumentException("minNumber must be less than or equal to maxNumber.");

        // Calculate the range between the min and max values
        var range = max - min + 1;

        // Calculate the byte size required to represent the range
        var byteSize = range.ToByteArray().Length;

        // Generate random bytes that fit within the byte size
        var randomBytes = Randomizer.Bytes(byteSize);

        // Convert the random bytes into a BigInteger
        var randomBigInt = new BigInteger(randomBytes);

        // Ensure the generated BigInteger is within the range [minNumber, maxNumber]
        // We use the modulus operation to ensure that the number stays within the range
        randomBigInt = System.Numerics.BigInteger.Abs(randomBigInt % range) + min;

        // Return the result as a string
        return randomBigInt.ToString(CultureInfo.InvariantCulture);
    }

    public virtual string Float(float minNumber = float.MinValue, float maxNumber = float.MaxValue,
        int decimalPlaces = 2)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        if (decimalPlaces < 0) throw new ArgumentException("Precision should be a non-negative integer.");

        return Randomizer.Float(minNumber, maxNumber).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
    }

    public virtual string Double(double minNumber = double.MinValue, double maxNumber = double.MaxValue,
        int decimalPlaces = 2)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        if (decimalPlaces < 0) throw new ArgumentException("Precision should be a non-negative integer.");

        return Randomizer.Double(minNumber, maxNumber).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
    }

    public virtual string Decimal(decimal minNumber = decimal.MinValue, decimal maxNumber = decimal.MaxValue,
        int decimalPlaces = 2)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        if (decimalPlaces < 0) throw new ArgumentException("Precision should be a non-negative integer.");

        return Randomizer.Decimal(minNumber, maxNumber).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
    }

    public virtual string Binary(int minNumber = 0, int maxNumber = 255)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        return Convert.ToString(Randomizer.Number(minNumber, maxNumber), 2);
    }

    public virtual string Octal(int minNumber = 0, int maxNumber = 255)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        return Convert.ToString(Randomizer.Number(minNumber, maxNumber), 8);
    }

    public virtual string Hexadecimal(int minNumber = 0, int maxNumber = 255)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        return Convert.ToString(Randomizer.Number(minNumber, maxNumber), 16);
    }

    public virtual string RomanNumeral(int minNumber = 1, int maxNumber = 3999)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        if (minNumber < 1) throw new ArgumentException("Min value should be greater than 0.");

        if (maxNumber > 3999) throw new ArgumentException("Max value should be less than 4000.");

        var number = Randomizer.Number(minNumber, maxNumber);

        var romanNumerals = new[]
        {
            new { Value = 1000, Numeral = "M" },
            new { Value = 900, Numeral = "CM" },
            new { Value = 500, Numeral = "D" },
            new { Value = 400, Numeral = "CD" },
            new { Value = 100, Numeral = "C" },
            new { Value = 90, Numeral = "XC" },
            new { Value = 50, Numeral = "L" },
            new { Value = 40, Numeral = "XL" },
            new { Value = 10, Numeral = "X" },
            new { Value = 9, Numeral = "IX" },
            new { Value = 5, Numeral = "V" },
            new { Value = 4, Numeral = "IV" },
            new { Value = 1, Numeral = "I" }
        };

        var result = string.Empty;

        foreach (var roman in romanNumerals)
            while (number >= roman.Value)
            {
                result += roman.Numeral;
                number -= roman.Value;
            }

        return result;
    }

    public virtual string Percent(int minNumber = 0, int maxNumber = 100, int decimalPlaces = 2,
        bool includeSymbol = true)
    {
        if (maxNumber <= minNumber)
            throw new ArgumentException($"Max {maxNumber} should be greater than min {minNumber}.");

        if (decimalPlaces < 0)
            throw new ArgumentException("Precision should be a non-negative integer.");

        return Randomizer.Float(minNumber, maxNumber).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) +
               (includeSymbol ? "%" : string.Empty);
    }

    public virtual long PrimeNumber(long minNumber = 1, long maxNumber = long.MaxValue)
    {
        if (minNumber < 0) throw new ArgumentException("Min value should be greater than or equal to 0.");

        if (maxNumber < 0) throw new ArgumentException("Max value should be greater than or equal to 0.");

        if (minNumber > maxNumber) throw new ArgumentException("Min value should be less than or equal to max value.");

        // Generate a random number within the range
        var prime = Randomizer.Long(minNumber, maxNumber);

        // Keep finding the next prime until it's valid
        while (!IsPrime(prime)) prime = Randomizer.Long(minNumber, maxNumber);

        return prime;

        // Inline prime check
        bool IsPrime(long number)
        {
            if (number < 2) return false;
            for (long i = 2; i * i <= number; i++)
                if (number % i == 0)
                    return false;
            return true;
        }
    }
}