using System.Text;
using System.Text.RegularExpressions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum DataUriTypeEnum
{
    SvgUri,
    SvgBase64
}

public class ImageModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Image";

    public virtual string GitHubAvatar()
    {
        return $"https://avatars.githubusercontent.com/u/{Randomizer.Number(0, 100000000)}";
    }

    public virtual string UrlLoremFlickr(int width = 640, int height = 480, string category = "")
    {
        if (width is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be between 1 and 3999.");

        if (height is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be between 1 and 3999.");

        return
            $"https://loremflickr.com/{width}/{height}{(string.IsNullOrEmpty(category) ? "" : $"/{category}")}?lock={Randomizer.Number(0, int.MaxValue)}";
    }

    public virtual string UrlPicsumPhotos(int width = 640, int height = 480, bool grayscale = false, int blur = 0)
    {
        if (width is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be between 1 and 3999.");

        if (height is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be between 1 and 3999.");

        if (blur is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(blur), blur, "Blur must be between 0 and 10.");

        // Build the base URL
        var url = $"https://picsum.photos/seed/{Randomizer.AlphaNumeric(Randomizer.Number(5, 25))}/{width}/{height}";

        // Add query parameters if necessary
        if (!grayscale && blur is < 1 or > 10) return url;

        url += "?";

        if (grayscale) url += "grayscale";

        if (grayscale && blur is >= 1 and <= 10) url += "&";

        if (blur is >= 1 and <= 10) url += $"blur={blur}";

        return url;
    }

    public virtual string Url(int width = 640, int height = 480)
    {
        if (width is < 0 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be between 0 and 3999.");

        if (height is < 0 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be between 0 and 3999.");

        if (width == 0) width = Randomizer.Number(1, 3999);

        if (height == 0) height = Randomizer.Number(1, 3999);

        return Randomizer.Bool()
            ? UrlLoremFlickr(width, height)
            : UrlPicsumPhotos(width, height);
    }

    public virtual string DataUri(int width = 640, int height = 480, string color = "",
        DataUriTypeEnum dataUriType = DataUriTypeEnum.SvgUri)
    {
        if (width is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be between 1 and 3999.");

        if (height is < 1 or > 3999)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be between 1 and 3999.");

        if (string.IsNullOrEmpty(color))
            color = $"#{Randomizer.Number(0x1000000):X6}";
        else if (!Regex.IsMatch(color, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
            throw new ArgumentException("Invalid color format. Must be a hex color code.", nameof(color));

        // Generate SVG string
        var svgString = $@"
            <svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1"" baseProfile=""full"" width=""{width}"" height=""{height}"">
                <rect width=""100%"" height=""100%"" fill=""{color}""/>
                <text x=""{width / 2}"" y=""{height / 2}"" font-size=""20"" alignment-baseline=""middle"" text-anchor=""middle"" fill=""white"">
                    {width}x{height}
                </text>
            </svg>";

        // Encode SVG as Data URI
        return dataUriType switch
        {
            DataUriTypeEnum.SvgUri => $"data:image/svg+xml;charset=UTF-8,{Uri.EscapeDataString(svgString)}",
            DataUriTypeEnum.SvgBase64 =>
                $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svgString))}",
            _ => throw new ArgumentOutOfRangeException(nameof(dataUriType), dataUriType,
                "Invalid image type specified.")
        };
    }
}