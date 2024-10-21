using System.Diagnostics;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace TableConverter.Views;

public partial class ConvertFilesPageView : UserControl
{
    private TextMate.Installation? InputFileTextEditorInstallation;

    private TextMate.Installation? OutputFileTextEditorInstallation;

    public ConvertFilesPageView()
    {
        InitializeComponent();

        var _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        InputFileTextEditorInstallation = InputFileTextEditor.InstallTextMate(_registryOptions);
        OutputFileTextEditorInstallation = OutputFileTextEditor.InstallTextMate(_registryOptions);

        var grammer = _registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".sql").Id);

        InputFileTextEditorInstallation.SetGrammar(grammer);
        OutputFileTextEditorInstallation.SetGrammar(grammer);

        InputFileTextEditor.DocumentChanged += (sender, e) =>
        {
            if (InputFileTextEditor.Document is null)
            {
                return;
            }

            var split = InputFileTextEditor.Document.FileName.Split('.');

            if (split.Length > 1)
            {
                var extension = split[^1];

                var language = _registryOptions.GetLanguageByExtension($".{extension}");

                if (language != null)
                {
                    var grammar = _registryOptions.GetScopeByLanguageId(language.Id);

                    InputFileTextEditorInstallation.SetGrammar(grammar);
                }
                else 
                {
                    InputFileTextEditorInstallation.SetGrammar("text");
                }
            }
        };

        OutputFileTextEditor.DocumentChanged += (sender, e) =>
        {
            if (OutputFileTextEditor.Document is null)
            {
                return;
            }

            var split = OutputFileTextEditor.Document.FileName.Split('.');

            if (split.Length > 1)
            {
                var extension = split[^1];

                var language = _registryOptions.GetLanguageByExtension($".{extension}");

                if (language != null)
                {
                    var grammar = _registryOptions.GetScopeByLanguageId(language.Id);

                    OutputFileTextEditorInstallation.SetGrammar(grammar);
                }
                else 
                {
                    OutputFileTextEditorInstallation.SetGrammar("text");
                }
            }
        };
    }
}