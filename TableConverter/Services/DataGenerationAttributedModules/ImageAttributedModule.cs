using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Image",
    "Module for generating random image-related data, including URLs for placeholder images and encoded image data.",
    "DataGenerationImageIcon")]
public class ImageAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ImageModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("GitHub Avatar", "Generates a random GitHub avatar URL.")]
    public override string GitHubAvatar()
    {
        return base.GitHubAvatar();
    }

    [DataGenerationModuleMethod("Url LoremFlickr",
        "Generates a random image URL from LoremFlickr with the specified width, height, and optional category. Default: width = 640, height = 480, category = \"\".")]
    public override string UrlLoremFlickr(int width = 640, int height = 480, string category = "")
    {
        return base.UrlLoremFlickr(width, height, category);
    }

    [DataGenerationModuleMethod("Url PicsumPhotos",
        "Generates a random image URL from PicsumPhotos with the specified width, height, optional grayscale, and blur effect. Default: width = 640, height = 480, grayscale = false, blur = 0.")]
    public override string UrlPicsumPhotos(int width = 640, int height = 480, bool grayscale = false, int blur = 0)
    {
        return base.UrlPicsumPhotos(width, height, grayscale, blur);
    }

    [DataGenerationModuleMethod("Url",
        "Generates a random image URL with the specified width and height. Default: width = 640, height = 480.")]
    public override string Url(int width = 640, int height = 480)
    {
        return base.Url(width, height);
    }

    [DataGenerationModuleMethod("Data Uri",
        "Generates a data URI representing an image with the specified width, height, color, and format (SVG or other types). Default: width = 640, height = 480, color = \"\", dataUriType = SvgUri.")]
    public override string DataUri(int width = 640, int height = 480, string color = "",
        DataUriTypeEnum dataUriType = DataUriTypeEnum.SvgUri)
    {
        return base.DataUri(width, height, color, dataUriType);
    }
}