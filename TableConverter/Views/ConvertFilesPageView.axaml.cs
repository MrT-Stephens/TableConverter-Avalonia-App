using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Utils;
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

        if (e.NewDocument.Lines.Any(line => line.TotalLength >= 3000))
        {
            installation?.SetGrammar(_Options?.GetScopeByLanguageId("plaintext"));
            return;
        }

        var extension = Path.GetExtension(e.NewDocument.FileName);

        if (string.IsNullOrEmpty(extension)) return;

        var language = _Options?.GetLanguageByExtension(extension);

        installation?.SetGrammar(language is not null
            ? _Options?.GetScopeByLanguageId(language.Id)
            : _Options?.GetScopeByLanguageId("plaintext"));
    }
}