using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TableConverter.Configuration;
using TableConverter.Interfaces;

namespace TableConverter.Services;

/// <inheritdoc cref="IDocumentSession" />
public sealed class DocumentSession : IDocumentSession
{
    /// <summary>
    /// Name of the file the open documents are written to, inside the configuration directory.
    /// </summary>
    private const string SessionFileName = "session.json";

    /// <summary>
    /// Version of the on disk format. It was raised when an entry grew a document type, so a session
    /// written by an older build is ignored instead of being misread.
    /// </summary>
    private const int CurrentVersion = 2;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IOptions<AppOptions> _appOptions;
    private readonly ILogger<DocumentSession> _logger;

    public DocumentSession(IOptions<AppOptions> appOptions, ILogger<DocumentSession> logger)
    {
        _appOptions = appOptions;
        _logger = logger;
    }

    private string SessionFilePath => Path.Combine(_appOptions.Value.BaseConfigPath, SessionFileName);

    /// <inheritdoc />
    public IReadOnlyList<DocumentSessionEntry> Load()
    {
        // The browser has no durable file system, so there is nothing to restore from.
        if (OperatingSystem.IsBrowser())
        {
            return [];
        }

        try
        {
            var sessionPath = SessionFilePath;

            if (!File.Exists(sessionPath))
            {
                return [];
            }

            using var stream = File.OpenRead(sessionPath);

            var session = JsonSerializer.Deserialize<SessionFile>(stream, SerializerOptions);

            if (session is null || session.Version > CurrentVersion || session.Entries is null)
            {
                return [];
            }

            return session.Entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.DocumentType))
                .ToArray();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The previous document session could not be read from '{Path}'.",
                SessionFilePath);

            return [];
        }
    }

    /// <inheritdoc />
    public void Save(string documentType, IEnumerable<JsonElement> states)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);
        ArgumentNullException.ThrowIfNull(states);

        if (OperatingSystem.IsBrowser())
        {
            return;
        }

        try
        {
            // The entries of the other document types are read back and kept, so saving one type never
            // discards the open documents of another.
            var entries = Load()
                .Where(entry => !string.Equals(entry.DocumentType, documentType, StringComparison.Ordinal))
                .ToList();

            entries.AddRange(states.Select(state => new DocumentSessionEntry(documentType, state)));

            Write(entries);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The document session could not be saved to '{Path}'.", SessionFilePath);
        }
    }

    private void Write(IEnumerable<DocumentSessionEntry> entries)
    {
        var sessionPath = SessionFilePath;

        Directory.CreateDirectory(_appOptions.Value.BaseConfigPath);

        var session = new SessionFile
        {
            Version = CurrentVersion,
            Entries = entries.ToArray(),
        };

        // Written through a temporary file so an interrupted write cannot leave a truncated session
        // behind that would lose every open document.
        var temporaryPath = sessionPath + ".tmp";

        using (var stream = File.Create(temporaryPath))
        {
            JsonSerializer.Serialize(stream, session, SerializerOptions);
        }

        File.Move(temporaryPath, sessionPath, true);
    }

    /// <summary>
    /// The on disk shape of the session file.
    /// </summary>
    private sealed class SessionFile
    {
        public int Version { get; set; } = CurrentVersion;

        public DocumentSessionEntry[]? Entries { get; set; }
    }
}

