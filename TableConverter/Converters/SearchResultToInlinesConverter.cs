using System;
using System.Globalization;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TableConverter.Contracts;

namespace TableConverter.Converters;

public class SearchResultToInlinesConverter : IValueConverter
{
    public static readonly SearchResultToInlinesConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TableSearchResult data)
            return new BindingNotification("Value is not TableSearchResult");

        IBrush brush;
        
        if (parameter is Brush parameterBrush)
        {
            brush = parameterBrush;
        }
        else
        {
            brush = Brushes.Yellow;
        }

        var text = data.Value ?? "";
        var highlight = data.FoundValue;

        var inlines = new InlineCollection();

        if (string.IsNullOrWhiteSpace(highlight))
        {
            inlines.Add(new Run(text));
            return inlines;
        }

        const StringComparison comparison = StringComparison.Ordinal;
        var index = 0;
        int matchIndex;

        while ((matchIndex = text.IndexOf(highlight, index, comparison)) >= 0)
        {
            // Before the match
            if (matchIndex > index)
                inlines.Add(new Run(text.Substring(index, matchIndex - index)));

            // Match span
            var span = new Span
            {
                Foreground = brush,
                FontWeight = FontWeight.Bold
            };
            
            span.Inlines.Add(new Run(text.Substring(matchIndex, highlight.Length)));
            inlines.Add(span);

            // Continue search after the match
            index = matchIndex + highlight.Length;
        }

        // Remaining text
        if (index < text.Length)
            inlines.Add(new Run(text[index..]));
        
        return inlines;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}