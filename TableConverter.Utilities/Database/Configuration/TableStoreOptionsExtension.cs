using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Configuration;

/// <summary>
/// Carries the table store a <see cref="Contexts.TableStoreDbContext"/> is bound to as part of its
/// <see cref="DbContextOptions"/>.
/// </summary>
/// <remarks>
/// EF Core factories are bound to a single set of options, so the identity of the data source (its
/// path), and the services the context needs in order to raise change notifications, travel with
/// the options instead of being passed to the context constructor. This lets EF Core itself create
/// the context instances.
/// </remarks>
public sealed class TableStoreOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _Info;

    public TableStoreOptionsExtension(string path, IEventManager eventManager)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A table store path must be provided.", nameof(path));
        }

        Path = path;
        EventManager = eventManager;
        SourceId = GuidUtility.Create(GuidUtility.UrlNamespace, path);
    }

    /// <summary>
    /// The path of the table store (a SQLite file on desktop, an in-memory store name in the browser).
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The event manager used to publish table store change notifications.
    /// </summary>
    public IEventManager EventManager { get; }

    /// <summary>
    /// A stable identifier for the table store, derived from <see cref="Path"/>.
    /// </summary>
    public Guid SourceId { get; }

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info => _Info ??= new ExtensionInfo(this);

    /// <inheritdoc />
    public void ApplyServices(IServiceCollection services)
    {
        // Nothing to add: the extension only carries state into the context, it does not provide services.
    }

    /// <inheritdoc />
    public void ApplyDefaults(IDbContextOptions options)
    {
    }

    /// <inheritdoc />
    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(TableStoreOptionsExtension extension)
        : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "TableStore ";

        // The table store identity does not influence which services a context needs, so every
        // table store can share the same internal service provider.
        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
