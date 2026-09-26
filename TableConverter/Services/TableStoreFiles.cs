using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TableConverter.Configuration;
using TableConverter.Contracts;
using TableConverter.Interfaces;

namespace TableConverter.Services;

/// <inheritdoc cref="ITableStoreFiles" />
public sealed class TableStoreFiles : ITableStoreFiles
{
    /// <summary>
    /// SQLite keeps its write ahead log and shared memory next to the database file. They are only
    /// meaningful while the database exists, so they are removed with it.
    /// </summary>
    private static readonly string[] SideCarSuffixes = ["-wal", "-shm"];

    private readonly IOptions<AppOptions> _appOptions;
    private readonly ILogger<TableStoreFiles> _logger;

    public TableStoreFiles(IOptions<AppOptions> appOptions, ILogger<TableStoreFiles> logger)
    {
        _appOptions = appOptions;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Delete(string path)
    {
        if (OperatingSystem.IsBrowser() || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            // The side cars have to go first: deleting the database file while a -wal file is still
            // present would make SQLite replay that log into a brand new database.
            foreach (var suffix in SideCarSuffixes)
            {
                var sideCar = path + suffix;

                if (File.Exists(sideCar))
                {
                    File.Delete(sideCar);
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The table store '{Path}' could not be deleted.", path);
        }
    }

    /// <inheritdoc />
    public void PruneOrphaned(IEnumerable<string> keepPaths)
    {
        if (OperatingSystem.IsBrowser())
        {
            return;
        }

        var documentsPath = _appOptions.Value.BaseDocumentsPath;

        if (!Directory.Exists(documentsPath))
        {
            return;
        }

        var keep = keepPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Normalise)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var store in Directory.EnumerateFiles(documentsPath, TableStoreFile.SearchPattern))
            {
                if (!TableStoreFile.IsApplicationGeneratedName(Path.GetFileName(store)) || keep.Contains(Normalise(store)))
                {
                    continue;
                }

                _logger.LogInformation("Removing orphaned table store '{Path}'.", store);

                Delete(store);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Orphaned table stores in '{Path}' could not be cleaned up.", documentsPath);
        }
    }

    private static string Normalise(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            // A malformed path can never match a file on disk, so it is compared as it was given.
            return path;
        }
    }
}

