using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TableConverter.DataGeneration.Exceptions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum EmojiTypeEnum
{
    People,
    Nature,
    Food,
    Activity,
    Travel,
    Objects,
    Symbols,
    Flags
}

public enum HttpStatusCodeTypeEnum
{
    Informational,
    Success,
    Redirection,
    ClientError,
    ServerError
}

public enum Ipv4NetworkEnum
{
    /**
     * Equivalent to: `0.0.0.0/0`
     */
    Any,

    /**
     * Equivalent to: `127.0.0.0/8`
     *
     * @see [RFC1122](https://www.rfc-editor.org/rfc/rfc1122)
     */
    Loopback,

    /**
     * Equivalent to: `10.0.0.0/8`
     *
     * @see [RFC1918](https://www.rfc-editor.org/rfc/rfc1918)
     */
    PrivateA,

    /**
     * Equivalent to: `172.16.0.0/12`
     *
     * @see [RFC1918](https://www.rfc-editor.org/rfc/rfc1918)
     */
    PrivateB,

    /**
     * Equivalent to: `192.168.0.0/16`
     *
     * @see [RFC1918](https://www.rfc-editor.org/rfc/rfc1918)
     */
    PrivateC,

    /**
     * Equivalent to: `192.0.2.0/24`
     *
     * @see [RFC5737](https://www.rfc-editor.org/rfc/rfc5737)
     */
    TestNet1,

    /**
     * Equivalent to: `198.51.100.0/24`
     *
     * @see [RFC5737](https://www.rfc-editor.org/rfc/rfc5737)
     */
    TestNet2,

    /**
     * Equivalent to: `203.0.113.0/24`
     *
     * @see [RFC5737](https://www.rfc-editor.org/rfc/rfc5737)
     */
    TestNet3,

    /**
     * Equivalent to: `169.254.0.0/16`
     *
     * @see [RFC3927](https://www.rfc-editor.org/rfc/rfc3927)
     */
    LinkLocal,

    /**
     * Equivalent to: `224.0.0.0/4`
     *
     * @see [RFC5771](https://www.rfc-editor.org/rfc/rfc5771)
     */
    Multicast
}

public enum ProtocolTypeEnum
{
    Http,
    Https,
    Any
}

public partial class InternetModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    private static readonly IReadOnlyDictionary<Ipv4NetworkEnum, string> Ipv4Networks =
        new Dictionary<Ipv4NetworkEnum, string>
        {
            { Ipv4NetworkEnum.Any, "0.0.0.0/0" },
            { Ipv4NetworkEnum.Loopback, "127.0.0.0/8" },
            { Ipv4NetworkEnum.PrivateA, "10.0.0.0/8" },
            { Ipv4NetworkEnum.PrivateB, "172.16.0.0/12" },
            { Ipv4NetworkEnum.PrivateC, "192.168.0.0/16" },
            { Ipv4NetworkEnum.TestNet1, "192.0.2.0/24" },
            { Ipv4NetworkEnum.TestNet2, "198.51.100.0/24" },
            { Ipv4NetworkEnum.TestNet3, "203.0.113.0/24" },
            { Ipv4NetworkEnum.LinkLocal, "169.254.0.0/16" },
            { Ipv4NetworkEnum.Multicast, "224.0.0.0/4" }
        };

    public override string ModuleName => "Internet";

    public virtual string Color(int r = 255, int g = 255, int b = 255)
    {
        if (r is < 0 or > 255)
            FakerArgumentOutOfRangeException.CreateException<object>(r, "Value must be between 0 and 255");

        if (g is < 0 or > 255)
            FakerArgumentOutOfRangeException.CreateException<object>(g, "Value must be between 0 and 255");

        if (b is < 0 or > 255)
           FakerArgumentOutOfRangeException.CreateException<object>(b, "Value must be between 0 and 255");

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public virtual string DisplayName(string firstName = "", string lastName = "")
    {
        var separator = Randomizer.GetRandomElement(['.', '_', '-']);

        var internalFirstName = string.IsNullOrEmpty(firstName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Generic)
            : firstName;

        var internalLastName = string.IsNullOrEmpty(lastName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.LastName.Generic)
            : lastName;

        var disambiguator = Randomizer.Number(0, 99);

        var switchValue = Randomizer.Number(0, 2);

        return (switchValue switch
        {
            0 => $"{internalFirstName}{disambiguator}",
            1 => $"{internalFirstName}{separator}{internalLastName}",
            2 => $"{internalFirstName}{separator}{internalLastName}{disambiguator}",
            _ => FakerArgumentException.CreateException<string>(switchValue, "Invalid display name switch value")
        }).Replace(' ', separator);
    }

    public virtual string DomainName()
    {
        return $"{DomainWord()}.{DomainSuffix()}";
    }

    public virtual string DomainWord()
    {
        var adjective = Randomizer.GetRandomElement(Locale.Word.Value.Adjective);
        var noun = Randomizer.GetRandomElement(Locale.Word.Value.Noun);

        return $"{MakeValidDomainWordSlug(adjective)}-{MakeValidDomainWordSlug(noun)}".ToLower(CultureInfo
            .InvariantCulture);
    }

    public virtual string DomainSuffix()
    {
        return Randomizer.GetWeightedValue(Locale.Internet.Value.DomainSuffix);
    }

    public virtual string Username(string firstName = "", string lastName = "")
    {
        var internalFirstName = string.IsNullOrEmpty(firstName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Generic)
            : firstName;

        var internalLastName = string.IsNullOrEmpty(lastName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.LastName.Generic)
            : lastName;

        var separator = Randomizer.GetRandomElement(['.', '_', '-']);
        var disambiguator = Randomizer.Number(0, 99);

        var switchValue = Randomizer.Number(0, 2);

        var result = (switchValue switch
        {
            0 => $"{internalFirstName}{disambiguator}",
            1 => $"{internalFirstName}{separator}{internalLastName}",
            2 => $"{internalFirstName}{separator}{internalLastName}{disambiguator}",
            _ => FakerArgumentException.CreateException<string>(switchValue, "Invalid username switch value")
        }).Normalize(NormalizationForm.FormKD);

        result = DiacriticalMarksRegex().Replace(result, string.Empty);

        result = string.Join(string.Empty, result.ToCharArray().Select(character =>
        {
            if (LanguageMapping.CharacterMapping.TryGetValue(character.ToString(CultureInfo.InvariantCulture),
                    out var replacement))
                return replacement;

            var charCode = (int)character;

            if (charCode < 0x80) return character.ToString(CultureInfo.InvariantCulture);

            const string base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

            var stringBuilder = new StringBuilder();

            do
            {
                stringBuilder.Insert(0, base36Chars[charCode % 36]);
                charCode /= 36;
            } while (charCode > 0);

            return stringBuilder.ToString();
        }));

        result = result.Replace("'", string.Empty);
        result = result.Replace(" ", string.Empty);

        return result;
    }

    public virtual string Email(string firstName = "", string lastName = "", string provider = "",
        bool allowSpecialChars = false)
    {
        var internalFirstName = string.IsNullOrEmpty(firstName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Generic)
            : firstName;

        var internalLastName = string.IsNullOrEmpty(lastName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.LastName.Generic)
            : lastName;

        var internalProvider = string.IsNullOrEmpty(provider)
            ? Randomizer.GetWeightedValue(Locale.Internet.Value.FreeEmail)
            : provider;

        var localPart = Username(internalFirstName, internalLastName);

        // Replace any character that is NOT A-Z, a-z, 0-9, ., _, +, or - with an empty string
        localPart = AlphaNumericRegex().Replace(localPart, string.Empty);

        localPart = localPart.Length > 50 ? localPart.Substring(0, 50) : localPart;

        if (allowSpecialChars)
        {
            // Define possible username characters and special characters
            char[] usernameChars = ['.', '_', '-'];
            char[] specialChars =
                ['.', '!', '#', '$', '%', '&', '\'', '*', '+', '-', '/', '=', '?', '^', '_', '`', '{', '|', '}', '~'];

            // Choose a random username character and a random special character
            var randomUsernameChar = Randomizer.GetRandomElement(usernameChars);
            var randomSpecialChar = Randomizer.GetRandomElement(specialChars);

            // Replace the random username character with the random special character
            localPart = localPart.Replace(randomUsernameChar, randomSpecialChar);
        }

        // Remove consecutive '.' characters (replace with a single '.')
        localPart = ConsecutiveDotsRegex().Replace(localPart, ".");

        // Remove leading and trailing '.' characters using Trim
        localPart = localPart.Trim('.');

        return $"{localPart}@{internalProvider}";
    }

    public virtual string Emoji(EmojiTypeEnum emojiType = EmojiTypeEnum.People)
    {
        return emojiType switch
        {
            EmojiTypeEnum.People => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Person),
            EmojiTypeEnum.Nature => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Nature),
            EmojiTypeEnum.Food => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Food),
            EmojiTypeEnum.Activity => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Activity),
            EmojiTypeEnum.Travel => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Travel),
            EmojiTypeEnum.Objects => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Object),
            EmojiTypeEnum.Symbols => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Symbol),
            EmojiTypeEnum.Flags => Randomizer.GetRandomElement(Locale.Internet.Value.Emoji.Flag),
            _ => FakerArgumentException.CreateException<string>(emojiType, "Invalid emoji type")
        };
    }

    public virtual string ExampleEmail(string firstName = "", string lastName = "", bool allowSpecialChars = false)
    {
        var internalFirstName = string.IsNullOrEmpty(firstName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Generic)
            : firstName;

        var internalLastName = string.IsNullOrEmpty(lastName)
            ? Randomizer.GetRandomElement(Locale.Person.Value.LastName.Generic)
            : lastName;

        var provider = Randomizer.GetWeightedValue(Locale.Internet.Value.FreeEmail);

        return Email(internalFirstName, internalLastName, provider, allowSpecialChars);
    }

    public virtual string HttpMethod()
    {
        return Randomizer.GetRandomElement(["GET", "POST", "PUT", "PATCH", "DELETE"]);
    }

    public virtual string HttpStatusCode(HttpStatusCodeTypeEnum type = HttpStatusCodeTypeEnum.Informational)
    {
        return type switch
        {
            HttpStatusCodeTypeEnum.Informational => Randomizer.GetRandomElement(Locale.Internet.Value.HttpStatusCode
                .Informational),
            HttpStatusCodeTypeEnum.Success => Randomizer.GetRandomElement(Locale.Internet.Value.HttpStatusCode.Success),
            HttpStatusCodeTypeEnum.Redirection => Randomizer.GetRandomElement(Locale.Internet.Value.HttpStatusCode
                .Redirection),
            HttpStatusCodeTypeEnum.ClientError => Randomizer.GetRandomElement(Locale.Internet.Value.HttpStatusCode
                .ClientError),
            HttpStatusCodeTypeEnum.ServerError => Randomizer.GetRandomElement(Locale.Internet.Value.HttpStatusCode
                .ServerError),
            _ => FakerArgumentException.CreateException<string>(type, "Invalid http status code type")
        };
    }

    public virtual string Ipv4(string cidr = "", Ipv4NetworkEnum network = Ipv4NetworkEnum.Any)
    {
        if (string.IsNullOrEmpty(cidr))
            cidr = Ipv4Networks[network];
        else if (!CidrRegex().IsMatch(cidr))
            FakerArgumentException.CreateException<object>(cidr, "CIDR must be in the format 'x.x.x.x/y'");

        // Split CIDR into IP and mask
        var parts = cidr.Split('/');
        var ip = parts[0];
        var mask = int.Parse(parts[1]);

        // Parse IP into bytes
        var ipBytes = IPAddress.Parse(ip).GetAddressBytes();
        var maskBytes = new byte[4];

        // Create subnet mask in byte format
        for (var i = 0; i < 4; i++)
            if (mask >= 8)
            {
                maskBytes[i] = 0xFF;
                mask -= 8;
            }
            else
            {
                maskBytes[i] = (byte)(0xFF << (8 - mask));
                break;
            }

        // Calculate network address by applying mask
        var networkBytes = new byte[4];
        for (var i = 0; i < 4; i++) networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);

        // Generate a random host offset within the subnet range
        var hostBytes = new byte[4];
        for (var i = 0; i < 4; i++) hostBytes[i] = (byte)(~maskBytes[i] & Randomizer.Number(0, 256));

        // Combine network and host to generate final IP
        var finalIpBytes = new byte[4];
        for (var i = 0; i < 4; i++) finalIpBytes[i] = (byte)(networkBytes[i] | hostBytes[i]);

        // Convert to dotted-decimal format
        return new IPAddress(finalIpBytes).ToString();
    }

    public virtual string Ipv6()
    {
        return string.Join(":",
            Enumerable.Range(0, 8).Select(_ => Randomizer.Hexadecimal(4, HexCasing.Lower, string.Empty)));
    }

    public virtual string Ip()
    {
        return Randomizer.Bool() ? Ipv4() : Ipv6();
    }

    public virtual string JwtAlgorithm()
    {
        return Randomizer.GetRandomElement(Locale.Internet.Value.JwtAlgorithm);
    }

    public virtual string Mac(string separator = ":")
    {
        string[] separators = [":", "-", ""];

        if (string.IsNullOrEmpty(separator))
            separator = Randomizer.GetRandomElement(separators);

        if (!separators.Contains(separator))
            FakerArgumentException.CreateException<object>(separator, "Separator must be one of ':', '-', or an empty string");

        return string.Join(separator,
            Enumerable.Range(0, 6).Select(_ => Randomizer.Hexadecimal(2, HexCasing.Lower, string.Empty)));
    }

    /// <summary>
    ///     Generates a random password-like string. Do not use this method for generating actual passwords for users.
    ///     Since the source of randomness is not cryptographically secure, neither is this generator.
    /// </summary>
    /// <param name="length">The length of the password to generate. Defaults to 15.</param>
    /// <param name="memorable">Whether the generated password should be memorable. Defaults to false.</param>
    /// <param name="pattern">The pattern that all characters should match. Ignored if memorable is true. Defaults to `\\w`.</param>
    /// <param name="prefix">The prefix to use. Defaults to an empty string.</param>
    /// <returns>A randomly generated password string.</returns>
    public virtual string Password(int length = 15, bool memorable = false, string pattern = @"\w", string prefix = "")
    {
        if (length <= 0)
            FakerArgumentException.CreateException<int>(length, "Length must be greater than 0");

        if (prefix.Length >= length) return prefix;

        var vowel = new Regex("[aeiouAEIOU]$");
        var consonant = new Regex("[bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ]$");
        var charPattern = new Regex(pattern);

        return Generate(length, memorable, charPattern, prefix);

        string Generate(int remainingLength, bool isMemorable, Regex currentPattern, string currentPrefix)
        {
            if (currentPrefix.Length >= remainingLength) return currentPrefix;

            // Determine the pattern if memorable
            if (isMemorable) currentPattern = consonant.IsMatch(currentPrefix) ? vowel : consonant;

            // Generate a random character in the printable ASCII range
            var generatedChar = Randomizer.Char((char)33, (char)126); // Range 33-126 (printable ASCII)

            // If memorable, force lowercase
            if (isMemorable) generatedChar = char.ToLower(generatedChar);

            // Validate the character against the pattern
            if (!currentPattern.IsMatch(generatedChar.ToString()))
                return Generate(remainingLength, isMemorable, currentPattern, currentPrefix);

            // Append the character and continue
            return Generate(remainingLength, isMemorable, currentPattern, currentPrefix + generatedChar);
        }
    }

    public virtual string Port()
    {
        return Randomizer.Number(65535).ToString();
    }

    public virtual string Protocol()
    {
        return Randomizer.GetRandomElement(["http", "https"]);
    }

    public virtual string Url(bool appendSlash = false, ProtocolTypeEnum protocol = ProtocolTypeEnum.Any)
    {
        var protocolString = protocol switch
        {
            ProtocolTypeEnum.Http => "http",
            ProtocolTypeEnum.Https => "https",
            ProtocolTypeEnum.Any => Randomizer.GetRandomElement(["http", "https"]),
            _ => FakerArgumentException.CreateException<string>(protocol, "Invalid protocol")
        };

        return $"{protocolString}://{DomainName()}{(appendSlash ? "/" : string.Empty)}";
    }

    public virtual string UserAgent()
    {
        return Randomizer.GetRandomElement(Locale.Internet.Value.UserAgentPattern).Build(Faker, Locale, Randomizer);
    }

    private string MakeValidDomainWordSlug(string word)
    {
        var slug1 = Slugify(word);

        if (IsValidDomainWordSlug(slug1)) return slug1;

        var slug2 = Slugify();

        if (IsValidDomainWordSlug(slug2)) return slug2;

        return new string(Randomizer.Chars('a', 'z', Randomizer.Number(4, 8)));
    }

    private static bool IsValidDomainWordSlug(string slug)
    {
        return !string.IsNullOrEmpty(slug) && DomainSlugRegex().IsMatch(slug);
    }

    private static string Slugify(string input = "")
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Normalize to decompose characters (e.g., è -> e + `)
        var normalized = input.Normalize(NormalizationForm.FormKD);

        // Remove combining diacritical marks (e.g., ` from e + `)
        var withoutDiacritics = DiacriticalMarksRegex().Replace(normalized, string.Empty);

        // Replace spaces with hyphens
        var withHyphens = SpacesRegex().Replace(withoutDiacritics, "-");

        // Remove non-word characters except dots and hyphens
        var slug = NonWordCharactersRegex().Replace(withHyphens, string.Empty);

        return slug;
    }

    [GeneratedRegex(@"^[a-z][a-z-]*[a-z]$", RegexOptions.IgnoreCase)]
    private static partial Regex DomainSlugRegex();

    [GeneratedRegex(@"[\u0300-\u036F]")]
    private static partial Regex DiacriticalMarksRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    [GeneratedRegex(@"[^\w.-]+")]
    private static partial Regex NonWordCharactersRegex();

    [GeneratedRegex(@"[^A-Za-z0-9._+-]")]
    private static partial Regex AlphaNumericRegex();

    [GeneratedRegex(@"\.{2,}")]
    private static partial Regex ConsecutiveDotsRegex();

    [GeneratedRegex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\/\d{1,2}$")]
    private static partial Regex CidrRegex();
}