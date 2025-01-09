using System.Text;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration;

// Some code in this file is based of the code from the following URL:
// https://github.com/bchavez/Bogus/

public enum HexCasing
{
    Mixed,
    Upper,
    Lower
}

public class Randomizer(int? seed = null)
{
    private static readonly Lazy<object> Locker = new(() => new object(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Random Random = seed.HasValue ? new Random(seed.Value) : new Random();

    /// <summary>
    ///     Get an int from 0 to max.
    /// </summary>
    /// <param name="max">Upper bound, inclusive.</param>
    public int Number(int max)
    {
        return Number(0, max);
    }

    /// <summary>
    ///     Get an int from min to max.
    /// </summary>
    /// <param name="min">Lower bound, inclusive</param>
    /// <param name="max">Upper bound, inclusive</param>
    public int Number(int min, int max)
    {
        //lock any seed access, for thread safety.
        lock (Locker.Value)
        {
            // Adjust the range as needed to make max inclusive. The Random.Next function uses exclusive upper bounds.

            // If max can be extended by 1, just do that.
            if (max < int.MaxValue) return Random.Next(min, max + 1);

            // If max is exactly int.MaxValue, then check if min can be used to push the range out by one the other way.
            // If so, then we can simply add one to the result to put it back in the correct range.
            if (min > int.MinValue) return 1 + Random.Next(min - 1, max);

            // If we hit this line, then min is int.MinValue and max is int.MaxValue, which mean the caller wants a
            // number from a range spanning all possible values of int. The Random class only supports exclusive
            // upper bounds, period, and the upper bound must be specified as an int, so the best we can get in a
            // single call is a value in the range (int.MinValue, int.MaxValue - 1). Instead, what we do is get two
            // samples, each of which has just under 31 bits of entropy, and use 16 bits from each to assemble a
            // single 16-bit number.
            var sample1 = Random.Next();
            var sample2 = Random.Next();

            var topHalf = (sample1 >> 8) & 0xFFFF;
            var bottomHalf = (sample2 >> 8) & 0xFFFF;

            return (topHalf << 16) | bottomHalf;
        }
    }

    /// <summary>
    ///     Get a random double, between 0.0 and 1.0.
    /// </summary>
    /// <param name="min">Minimum, inclusive. Default 0.0</param>
    /// <param name="max">Maximum, exclusive. Default 1.0</param>
    public double Double(double min = 0.0d, double max = 1.0d)
    {
        //lock any seed access, for thread safety.
        lock (Locker.Value)
        {
            if (min == 0.0d && max == 1.0d)
                //use default implementation
                return Random.NextDouble();

            return Random.NextDouble() * (max - min) + min;
        }
    }

    /// <summary>
    ///     Generate a random long between MinValue and MaxValue.
    /// </summary>
    /// <param name="min">Min value, inclusive. Default long.MinValue</param>
    /// <param name="max">Max value, inclusive. Default long.MaxValue</param>
    public long Long(long min = long.MinValue, long max = long.MaxValue)
    {
        var range = (decimal)max - min; //use more bits?
        return Convert.ToInt64((decimal)Double() * range + min);
    }

    /// <summary>
    ///     Get a random sequence of digits.
    /// </summary>
    /// <param name="count">How many</param>
    /// <param name="minDigit">minimum digit, inclusive</param>
    /// <param name="maxDigit">maximum digit, inclusive</param>
    public int[] Digits(int count, int minDigit = 0, int maxDigit = 9)
    {
        if (maxDigit is > 9 or < 0)
            throw new ArgumentException("max digit can't be lager than 9 or smaller than 0", nameof(maxDigit));

        if (minDigit is > 9 or < 0)
            throw new ArgumentException("min digit can't be lager than 9 or smaller than 0", nameof(minDigit));

        var digits = new int[count];

        for (var i = 0; i < count; i++) digits[i] = Number(minDigit, maxDigit);

        return digits;
    }

    /// <summary>
    ///     Get a random decimal, between 0.0 and 1.0.
    /// </summary>
    /// <param name="min">Minimum, inclusive. Default 0.0</param>
    /// <param name="max">Maximum, exclusive. Default 1.0</param>
    public decimal Decimal(decimal min = 0.0m, decimal max = 1.0m)
    {
        return Convert.ToDecimal(Double()) * (max - min) + min;
    }

    /// <summary>
    ///     Get a random float, between 0.0 and 1.0.
    /// </summary>
    /// <param name="min">Minimum, inclusive. Default 0.0</param>
    /// <param name="max">Maximum, inclusive. Default 1.0</param>
    public float Float(float min = 0.0f, float max = 1.0f)
    {
        return Convert.ToSingle(Double() * (max - min) + min);
    }

    /// <summary>
    ///     Generate a random byte between 0 and 255.
    /// </summary>
    /// <param name="min">Min value, inclusive. Default byte.MinValue 0</param>
    /// <param name="max">Max value, inclusive. Default byte.MaxValue 255</param>
    public byte Byte(byte min = byte.MinValue, byte max = byte.MaxValue)
    {
        return Convert.ToByte(Number(min, max));
    }

    /// <summary>
    ///     Get a random sequence of bytes.
    /// </summary>
    /// <param name="count">The size of the byte array</param>
    public byte[] Bytes(int count)
    {
        var arr = new byte[count];

        lock (Locker.Value)
        {
            Random.NextBytes(arr);
        }

        return arr;
    }

    /// <summary>
    ///     Generate a random sbyte between -128 and 127.
    /// </summary>
    /// <param name="min">Min value, inclusive. Default sbyte.MinValue -128</param>
    /// <param name="max">Max value, inclusive. Default sbyte.MaxValue 127</param>
    public sbyte SByte(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
    {
        return Convert.ToSByte(Number(min, max));
    }

    /// <summary>
    ///     Generate a random char between MinValue and MaxValue.
    /// </summary>
    /// <param name="min">Min value, inclusive. Default char.MinValue</param>
    /// <param name="max">Max value, inclusive. Default char.MaxValue</param>
    public char Char(char min = char.MinValue, char max = char.MaxValue)
    {
        return Convert.ToChar(Number(min, max));
    }

    /// <summary>
    ///     Generate a random chars between MinValue and MaxValue.
    /// </summary>
    /// <param name="min">Min value, inclusive. Default char.MinValue</param>
    /// <param name="max">Max value, inclusive. Default char.MaxValue</param>
    /// <param name="count">The length of chars to return</param>
    public char[] Chars(char min = char.MinValue, char max = char.MaxValue, int count = 5)
    {
        var arr = new char[count];
        for (var i = 0; i < count; i++)
            arr[i] = Char(min, max);
        return arr;
    }

    /// <summary>
    ///     Generates a random alphanumeric string of the specified length.
    /// </summary>
    /// <param name="length">The length of the alphanumeric string to generate. Default is 5.</param>
    /// <returns>A randomly generated alphanumeric string.</returns>
    public string AlphaNumeric(int length = 5)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var result = new char[length];

        for (var i = 0; i < length; i++) result[i] = chars[Number(0, chars.Length - 1)];

        return new string(result);
    }

    /// <summary>
    ///     Get a random boolean.
    /// </summary>
    public bool Bool()
    {
        return Number(1) == 0;
    }

    /// <summary>
    ///     Get a random boolean.
    /// </summary>
    /// <param name="weight">The probability of true. Ranges from 0 to 1.</param>
    public bool Bool(float weight)
    {
        return Float() < weight;
    }

    /// <summary>
    ///     Generates a random hexadecimal string with various customization options.
    /// </summary>
    /// <param name="length">
    ///     Length of the hexadecimal string (excluding the prefix). Must be a positive integer.
    /// </param>
    /// <param name="casing">
    ///     Casing for the generated string: Mixed, Upper, or Lower. Default is Mixed.
    /// </param>
    /// <param name="prefix">
    ///     Prefix for the hexadecimal string. Default is "0x".
    /// </param>
    /// <returns>A randomly generated hexadecimal string based on the given options.</returns>
    public string Hexadecimal(int length = 1, HexCasing casing = HexCasing.Mixed, string prefix = "0x")
    {
        if (length < 0)
            throw new ArgumentException("Length must be non-negative.");

        if (length == 0)
            return prefix;

        // Array of hexadecimal characters
        char[] hexChars =
        [
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F'
        ];

        var result = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            // Select a random character based on casing
            var randomChar = hexChars[Number(0, casing == HexCasing.Mixed ? hexChars.Length - 1 : 15)];

            switch (casing)
            {
                case HexCasing.Upper:
                    randomChar = char.ToUpper(randomChar);
                    break;
                case HexCasing.Lower:
                    randomChar = char.ToLower(randomChar);
                    break;
                case HexCasing.Mixed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(casing), casing, null);
            }

            result.Append(randomChar);
        }

        return $"{prefix}{result}";
    }

    /// <summary>
    ///     Selects a value from a collection of weighted values based on their weights.
    /// </summary>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    /// <param name="values">
    ///     An immutable array of objects implementing the IWeightedValue<![CDATA[<T>]]> interface.
    ///     Each object contains a value of type T and an associated weight.
    /// </param>
    /// <returns>
    ///     A randomly selected value of type T, with the probability of selection proportional to its weight.
    /// </returns>
    public T GetWeightedValue<T>(IReadOnlyList<IWeightedValue<T>> values)
    {
        var totalWeight = values.Sum(x => x.Weight);
        var randomValue = Number(totalWeight);

        foreach (var value in values)
        {
            randomValue -= value.Weight;

            if (randomValue <= 0) return value.Value;
        }

        return values[^1].Value;
    }

    /// <summary>
    ///     Selects a random element from an immutable array.
    /// </summary>
    /// <param name="values">The array of elements.</param>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <returns>The random element which was selected.</returns>
    public T GetRandomElement<T>(IReadOnlyList<T> values)
    {
        return values[Number(values.Count - 1)];
    }

    /// <summary>
    ///     Replaces symbols in a pattern with random digits.
    ///     '#' is replaced with any digit (0-9), and '!' is replaced with digits (2-9).
    /// </summary>
    /// <param name="pattern">The pattern string.</param>
    /// <param name="symbol">The placeholder character for random digits (default is '#').</param>
    /// <returns>A string with replaced symbols.</returns>
    public string ReplaceSymbolsWithNumbers(string pattern, char symbol = '#')
    {
        var result = new char[pattern.Length];

        char[] digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        for (var i = 0; i < result.Length; i++)
            if (pattern[i] == symbol)
                result[i] = Number(9).ToString()[0];
            else if (pattern[i] == '!')
                result[i] = Number(9).ToString()[0];
            else
                result[i] = pattern[i];

        return new string(result);
    }

    /// <summary>
    ///     Replaces specific symbols in the input pattern with random characters based on predefined rules.
    /// </summary>
    /// <param name="pattern">
    ///     The input string containing symbols to replace. The valid symbols include:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>'#' - Will be replaced with a random digit (0-9).</description>
    ///         </item>
    ///         <item>
    ///             <description>'?' - Will be replaced with a random uppercase alphabetic character (a-z).</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 '•' - Will be replaced with either a random uppercase alphabetic character (a-z) or a random
    ///                 digit (0-9), chosen randomly.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </param>
    /// <returns>
    ///     A new string with the symbols replaced by random characters, or the original characters if no replacement rule
    ///     applies.
    /// </returns>
    public string ReplaceSymbols(string pattern)
    {
        var result = new char[pattern.Length];

        char[] alpha =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        ];

        char[] digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        for (var i = 0; i < result.Length; i++)
            result[i] = pattern[i] switch
            {
                '#' => GetRandomElement(digits),
                '?' => GetRandomElement(alpha),
                '•' => Bool() ? GetRandomElement(alpha) : GetRandomElement(digits),
                _ => pattern[i]
            };

        return new string(result);
    }

    /// <summary>
    ///     Generates a valid credit card number based on the provided pattern.
    /// </summary>
    /// <param name="pattern">Pattern for the credit card number (e.g., "#### #### #### L").</param>
    /// <param name="symbol">The placeholder character for random digits (default is '#').</param>
    /// <returns>A valid credit card number string.</returns>
    public string ReplaceCreditCardSymbols(string pattern, char symbol = '#')
    {
        // Step 1: Replace symbols in the pattern
        var cardNumber = ReplaceSymbolsWithNumbers(pattern, symbol);

        // Step 2: Calculate the Luhn check digit
        var luhnIndex = pattern.IndexOf('L');

        if (luhnIndex != -1)
            cardNumber = cardNumber.Remove(luhnIndex, 1).Insert(luhnIndex, LuhnCheckValue(cardNumber).ToString());

        return cardNumber;
    }

    /// <summary>
    ///     Calculates the Luhn check value for a string.
    /// </summary>
    /// <param name="str">The string to calculate the check digit for (with or without 'L').</param>
    /// <returns>The Luhn check digit.</returns>
    private static int LuhnCheckValue(string str)
    {
        var modifiedString = str.Replace("L", "0");
        var checksum = LuhnChecksum(modifiedString);
        return checksum == 0 ? 0 : 10 - checksum;
    }

    /// <summary>
    ///     Calculates the Luhn checksum value for the given string.
    /// </summary>
    /// <param name="str">The string to calculate the checksum for.</param>
    /// <returns>The checksum modulo 10.</returns>
    private static int LuhnChecksum(string str)
    {
        var sum = 0;
        var alternate = false;

        // Process characters from right to left
        for (var i = str.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(str[i])) continue; // Only process digits

            var n = int.Parse(str[i].ToString());

            if (alternate)
            {
                n *= 2; // Double every second digit

                if (n > 9) n = n % 10 + 1; // If > 9, sum the digits (e.g., 14 → 1 + 4)
            }

            sum += n;
            alternate = !alternate; // Alternate between doubling and not doubling
        }

        return sum % 10; // Return checksum modulo 10
    }
}