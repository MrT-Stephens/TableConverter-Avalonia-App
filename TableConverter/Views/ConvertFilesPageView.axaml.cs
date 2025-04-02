using System.IO;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using SukiUI;
using TextMateSharp.Grammars;

namespace TableConverter.Views;

public partial class ConvertFilesPageView : UserControl
{
    private readonly RegistryOptions? _Options;

    public ConvertFilesPageView()
    {
        InitializeComponent();

        _Options = new RegistryOptions(ThemeName.HighContrastDark);

        var inputInstallation = InputFileTextEditor.InstallTextMate(_Options);
        var outputInstallation = OutputFileTextEditor.InstallTextMate(_Options);

        InputFileTextEditor.DocumentChanged += (_, e) => DocumentOnChanged(e, inputInstallation);
        OutputFileTextEditor.DocumentChanged += (_, e) => DocumentOnChanged(e, outputInstallation);

        SukiTheme.GetInstance().OnBaseThemeChanged += variant =>
        {
            if (variant == ThemeVariant.Dark)
            {
                inputInstallation.SetTheme(_Options.LoadTheme(ThemeName.HighContrastDark));
                outputInstallation.SetTheme(_Options.LoadTheme(ThemeName.HighContrastDark));
            }
            else
            {
                inputInstallation.SetTheme(_Options.LoadTheme(ThemeName.HighContrastLight));
                outputInstallation.SetTheme(_Options.LoadTheme(ThemeName.HighContrastLight));
            }
        };
    }

    private void DocumentOnChanged(DocumentChangedEventArgs e, TextMate.Installation? installation)
    {
        if (e.NewDocument is null) return;

        var extension = Path.GetExtension(e.NewDocument.FileName);

        if (string.IsNullOrEmpty(extension)) return;

        var language = _Options?.GetLanguageByExtension(extension);

        installation?.SetGrammar(language is not null
            ? _Options?.GetScopeByLanguageId(language.Id)
            : _Options?.GetScopeByLanguageId("plaintext"));
    }
}