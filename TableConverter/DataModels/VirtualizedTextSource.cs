using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AvaloniaEdit.Document;

namespace TableConverter.DataModels;

public class VirtualizedTextSource : ITextSource
{
    private readonly string _FilePath;
    private readonly Dictionary<int, string> _Cache = new(); // Caches lines for fast access
    private const int CacheSize = 500; // Adjust cache size as needed
    private readonly int _TotalLines;
    
    public int TextLength => (int)new FileInfo(_FilePath).Length;

    public string Text => GetText(0, _TotalLines);
    
    public ITextSourceVersion Version => null!;

    public VirtualizedTextSource(string filePath)
    {
        _FilePath = filePath;
        _TotalLines = File.ReadLines(_FilePath).Count(); // Get total lines for navigation
    }

    public string GetText(int offset, int length)
    {
        return ReadLines(offset, length);
    }

    public string GetText(ISegment segment)
    {
        return GetText(segment.Offset, segment.Length);
    }

    public void WriteTextTo(TextWriter writer)
    {
        writer.Write(GetText(0, _TotalLines));
    }

    public void WriteTextTo(TextWriter writer, int offset, int length)
    {
        writer.Write(GetText(offset, length));
    }

    public int IndexOf(char c, int startIndex, int count)
    {
        return GetText(startIndex, count).IndexOf(c);
    }

    public int IndexOfAny(char[] anyOf, int startIndex, int count)
    {
        return GetText(startIndex, count).IndexOfAny(anyOf);
    }

    public int IndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
    {
        return GetText(startIndex, count).IndexOf(searchText, comparisonType);
    }

    public int LastIndexOf(char c, int startIndex, int count)
    {
        return GetText(startIndex, count).LastIndexOf(c);
    }

    public int LastIndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
    {
        return GetText(startIndex, count).LastIndexOf(searchText, comparisonType);
    }

    public ITextSource CreateSnapshot()
    {
        return new StringTextSource(GetText(0, _TotalLines));
    }

    public ITextSource CreateSnapshot(int offset, int length)
    {
        return new StringTextSource(GetText(offset, length));
    }

    public TextReader CreateReader()
    {
        return new StringReader(GetText(0, _TotalLines));
    }

    public TextReader CreateReader(int offset, int length)
    {
        return new StringReader(GetText(offset, length));
    }

    public char GetCharAt(int offset)
    {
        var text = ReadLines(offset, 1);
        return string.IsNullOrEmpty(text) ? '\0' : text[0];
    }

    private string ReadLines(int startLine, int count)
    {
        if (!File.Exists(_FilePath)) return string.Empty;
        if (startLine >= _TotalLines) return string.Empty;

        if (_Cache.TryGetValue(startLine, out var value))
            return value; // Return cached line if available

        var result = new StringBuilder();
        using var reader = new StreamReader(_FilePath);
        var currentLine = 0;

        while (currentLine < startLine && !reader.EndOfStream)
        {
            reader.ReadLine(); // Skip to startLine
            currentLine++;
        }

        for (var i = 0; i < count && !reader.EndOfStream; i++)
        {
            var line = reader.ReadLine();
            result.AppendLine(line);
            if (_Cache.Count < CacheSize) _Cache[startLine + i] = line!; // Cache the line
        }

        return result.ToString();
    }
}