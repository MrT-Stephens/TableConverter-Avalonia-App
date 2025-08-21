using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace TableConverter.Components.Extensions;

public class CodeEditor : TextEditor
{
    protected override Type StyleKeyOverride => typeof(TextEditor);

    public CodeEditor()
    {
        ShowLineNumbers = true;
        FlowDirection = FlowDirection.LeftToRight;
        WordWrap = false;
        Options = new TextEditorOptions()
        {
            AllowScrollBelowDocument = true,
            HighlightCurrentLine = false,
            IndentationSize = 5,
            EnableRectangularSelection = true,
            ShowBoxForControlCharacters = true,
            ShowEndOfLine = true,
            ShowSpaces = true,
            ShowTabs = true,
            RequireControlModifierForHyperlinkClick = false,
            EnableEmailHyperlinks = false,
            EnableHyperlinks = false
        };
        FontFamily = Application.Current?.Resources["SecondaryFontFamily"] as FontFamily 
            ?? throw new InvalidOperationException("SecondaryFontFamily not found in resources.");
        HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

        ActualThemeVariantChanged += (_, _) => UpdateEditorTheme();
    }

    protected override void OnDocumentChanged(DocumentChangedEventArgs e)
    {
        base.OnDocumentChanged(e);
        
        if (e.NewDocument is null) return;
        
        UpdateEditorTheme();
    }

    private void UpdateEditorTheme()
    {
        var theme = ActualThemeVariant == ThemeVariant.Light
                    ? ThemeName.LightPlus
                    : ThemeName.DarkPlus;

        var options = new RegistryOptions(theme);

        var installation = this.InstallTextMate(options);
        
        if (Document is null) return;

        if (Document.Lines.Any(line => line.TotalLength >= 3000))
        {
            installation?.SetGrammar(options?.GetScopeByLanguageId("plaintext"));
            return;
        }

        var extension = Path.GetExtension(Document.FileName);

        if (string.IsNullOrEmpty(extension)) return;

        var language = options?.GetLanguageByExtension(extension);

        installation?.SetGrammar(language is not null
            ? options?.GetScopeByLanguageId(language.Id)
            : options?.GetScopeByLanguageId("plaintext"));
    }
}
