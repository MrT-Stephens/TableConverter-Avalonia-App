using System.Globalization;
using System.Text.RegularExpressions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class SystemModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "System";

    public virtual string FileExtension(string mimeType = "")
    {
        if (string.IsNullOrEmpty(mimeType)) mimeType = Randomizer.GetRandomElement(Locale.System.Value.MimeType).Mime;

        if (Locale.System.Value.MimeType.Any(x => x.Mime == mimeType))
            return Randomizer.GetRandomElement(Locale.System.Value.MimeType.First(x => x.Mime == mimeType).Extensions);

        return Randomizer.GetRandomElement(Randomizer.GetRandomElement(Locale.System.Value.MimeType).Extensions);
    }

    public virtual string FileType()
    {
        return Randomizer.GetRandomElement(Locale.System.Value.MimeType).Mime.Split("/")[0];
    }

    public virtual string DirectoryPath()
    {
        return Randomizer.GetRandomElement(Locale.System.Value.DirectoryPath);
    }

    public virtual string CommonFileExtension()
    {
        return FileExtension(CommonFileMimeType());
    }

    public virtual string CommonFileType()
    {
        return Randomizer.GetRandomElement(["video", "audio", "image", "text", "application"]);
    }

    public virtual string CommonFileMimeType()
    {
        return Randomizer.GetRandomElement([
            "application/pdf",
            "audio/mpeg",
            "audio/wav",
            "image/png",
            "image/jpeg",
            "image/gif",
            "video/mp4",
            "video/mpeg",
            "text/html"
        ]);
    }

    public virtual string FileName(string extension = "")
    {
        if (string.IsNullOrEmpty(extension)) extension = FileExtension();

        var word = Faker.Word.Words(1, 3).ToLower(CultureInfo.InvariantCulture);

        word = Regex.Replace(word, @"\W", "_");

        return $"{word}.{extension}";
    }

    public virtual string CommonFileName(string extension = "")
    {
        if (string.IsNullOrEmpty(extension)) extension = CommonFileExtension();

        return FileName(extension);
    }

    public virtual string FilePath()
    {
        return $"{DirectoryPath()}/{FileName()}";
    }

    public virtual string Semver()
    {
        return $"{Randomizer.Number(0, 9)}.{Randomizer.Number(0, 9)}.{Randomizer.Number(0, 9)}";
    }
}