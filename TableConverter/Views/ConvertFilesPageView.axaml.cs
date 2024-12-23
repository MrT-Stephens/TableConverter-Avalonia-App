using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using SukiUI;
using TextMateSharp.Grammars;

namespace TableConverter.Views;

public partial class ConvertFilesPageView : UserControl
{
    private readonly RegistryOptions? _options;

    public ConvertFilesPageView()
    {
        InitializeComponent();
        
        _options = new RegistryOptions(ThemeName.HighContrastDark);
        
        var inputInstallation = InputFileTextEditor.InstallTextMate(_options);
        var outputInstallation = OutputFileTextEditor.InstallTextMate(_options);
        
        InputFileTextEditor.DocumentChanged += (_, e) => DocumentOnChanged(e, inputInstallation);
        OutputFileTextEditor.DocumentChanged += (_, e) => DocumentOnChanged(e, outputInstallation);

        SukiTheme.GetInstance().OnBaseThemeChanged += variant =>
        {
            if (variant == ThemeVariant.Dark)
            {
                inputInstallation.SetTheme(_options.LoadTheme(ThemeName.HighContrastDark));
                outputInstallation.SetTheme(_options.LoadTheme(ThemeName.HighContrastDark));
            }
            else
            {
                inputInstallation.SetTheme(_options.LoadTheme(ThemeName.HighContrastLight));
                outputInstallation.SetTheme(_options.LoadTheme(ThemeName.HighContrastLight));
            }
        };
    }
    
    private void DocumentOnChanged(DocumentChangedEventArgs e, TextMate.Installation? installation)
    {
        if (e.NewDocument is null)
        {
            return;
        }
        
        var extension = Path.GetExtension(e.NewDocument.FileName);
        
        if (string.IsNullOrEmpty(extension))
        {
            return;
        }
        
        var language = _options?.GetLanguageByExtension(extension);

        installation?.SetGrammar(language is not null
            ? _options?.GetScopeByLanguageId(language.Id)
            : _options?.GetScopeByLanguageId("plaintext"));
    }
}