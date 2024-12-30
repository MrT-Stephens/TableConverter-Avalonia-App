using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Internet",
    "Module for generating internet-related data such as URLs, emails, IP addresses, and user agents.",
    "DataGenerationInternetIcon")]
public class InternetAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : InternetModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Color", "Generates a hexadecimal color code from RGB values. Example: '#ff5733'")]
    public override string Color(int r = 255, int g = 255, int b = 255)
    {
        return base.Color(r, g, b);
    }

    [DataGenerationModuleMethod("Display Name",
        "Generates a display name by combining a first name and last name. Example: 'John Doe'")]
    public override string DisplayName(string firstName = "", string lastName = "")
    {
        return base.DisplayName(firstName, lastName);
    }

    [DataGenerationModuleMethod("Domain Name", "Generates a random domain name. Example: 'example.com'")]
    public override string DomainName()
    {
        return base.DomainName();
    }

    [DataGenerationModuleMethod("Domain Word",
        "Generates a random domain word (e.g., the name portion of a domain). Example: 'example'")]
    public override string DomainWord()
    {
        return base.DomainWord();
    }

    [DataGenerationModuleMethod("Domain Suffix", "Generates a random domain suffix. Example: '.com', '.net'")]
    public override string DomainSuffix()
    {
        return base.DomainSuffix();
    }

    [DataGenerationModuleMethod("Username",
        "Generates a random username, optionally using first and last names. Example: 'jdoe123'")]
    public override string Username(string firstName = "", string lastName = "")
    {
        return base.Username(firstName, lastName);
    }

    [DataGenerationModuleMethod("Email", "Generates a random email address. Example: 'john.doe@example.com'")]
    public override string Email(string firstName = "", string lastName = "", string provider = "",
        bool allowSpecialChars = false)
    {
        return base.Email(firstName, lastName, provider, allowSpecialChars);
    }

    [DataGenerationModuleMethod("Emoji", "Generates an emoji based on the specified emoji type. Example: '😊'")]
    public override string Emoji(EmojiTypeEnum emojiType = EmojiTypeEnum.People)
    {
        return base.Emoji(emojiType);
    }

    [DataGenerationModuleMethod("Example Email",
        "Generates an example email address with a common provider. Example: 'user@example.com'")]
    public override string ExampleEmail(string firstName = "", string lastName = "", bool allowSpecialChars = false)
    {
        return base.ExampleEmail(firstName, lastName, allowSpecialChars);
    }

    [DataGenerationModuleMethod("Http Method", "Generates an HTTP method. Example: 'GET', 'POST'")]
    public override string HttpMethod()
    {
        return base.HttpMethod();
    }

    [DataGenerationModuleMethod("Http Status Code",
        "Generates an HTTP status code based on the specified type. Example: '200'")]
    public override string HttpStatusCode(HttpStatusCodeTypeEnum type = HttpStatusCodeTypeEnum.Informational)
    {
        return base.HttpStatusCode(type);
    }

    [DataGenerationModuleMethod("Ipv4", "Generates a random IPv4 address. Example: '192.168.0.1'")]
    public override string Ipv4(string cidr = "", Ipv4NetworkEnum network = Ipv4NetworkEnum.Any)
    {
        return base.Ipv4(cidr, network);
    }

    [DataGenerationModuleMethod("Ipv6",
        "Generates a random IPv6 address. Example: '2001:0db8:85a3:0000:0000:8a2e:0370:7334'")]
    public override string Ipv6()
    {
        return base.Ipv6();
    }

    [DataGenerationModuleMethod("Ip",
        "Generates a random IP address (IPv4 or IPv6). Example: '192.168.0.1' or '2001:0db8::7334'")]
    public override string Ip()
    {
        return base.Ip();
    }

    [DataGenerationModuleMethod("Jwt Algorithm",
        "Generates a random JSON Web Token (JWT) algorithm name. Example: 'HS256'")]
    public override string JwtAlgorithm()
    {
        return base.JwtAlgorithm();
    }

    [DataGenerationModuleMethod("Mac", "Generates a random MAC address. Example: '00:1A:2B:3C:4D:5E'")]
    public override string Mac(string separator = ":")
    {
        return base.Mac(separator);
    }

    [DataGenerationModuleMethod("Password",
        "Generates a random password with the specified length and characteristics. Example: 'Abc123!xyz'")]
    public override string Password(int length = 15, bool memorable = false, string pattern = "\\w", string prefix = "")
    {
        return base.Password(length, memorable, pattern, prefix);
    }

    [DataGenerationModuleMethod("Port", "Generates a random port number. Example: '8080'")]
    public override string Port()
    {
        return base.Port();
    }

    [DataGenerationModuleMethod("Protocol", "Generates a random protocol name. Example: 'http', 'https'")]
    public override string Protocol()
    {
        return base.Protocol();
    }

    [DataGenerationModuleMethod("Url", "Generates a random URL. Example: 'https://www.example.com'")]
    public override string Url(bool appendSlash = false, ProtocolTypeEnum protocol = ProtocolTypeEnum.Any)
    {
        return base.Url(appendSlash, protocol);
    }

    [DataGenerationModuleMethod("User Agent",
        "Generates a random user agent string. Example: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'")]
    public override string UserAgent()
    {
        return base.UserAgent();
    }
}