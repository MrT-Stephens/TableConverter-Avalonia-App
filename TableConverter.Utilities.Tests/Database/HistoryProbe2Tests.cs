using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using Xunit.Abstractions;

namespace TableConverter.Utilities.Tests.Database;

public class HistoryProbe2Tests(ITestOutputHelper output)
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IEventManager, EventManager>();
        services.AddTableStoreDatabase();

        return services.BuildServiceProvider();
    }

    private static async Task<string> TrimWithRegions(
        ServiceProvider provider,
        string label,
        bool captureRows,
        bool captureColumns)
    {
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        await using (var db = await factory.CreateDbContextAsync(path))
        {
            await using var sink = TableStoreRowSink.Create(db);

            await sink.BeginAsync([" A ", "B"]);
            await sink.WriteRowAsync([" a1 ", "b1 "]);
            await sink.CompleteAsync();
        }

        string result;

        await using (var db = await factory.CreateDbContextAsync(path))
        {
            var maintenance = TableStoreMaintenance.Create(db);

            await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Trimmed");

            var rows = await maintenance.GetRowIdsToTrimAsync();

            if (captureRows)
            {
                await edit.CaptureBeforeAsync(TableRegion.Rows(rows));
            }

            if (captureColumns)
            {
                await edit.CaptureBeforeAsync(TableRegion.Columns());
            }

            await maintenance.TrimAsync();

            var entry = await edit.CommitAsync();

            result = entry?.Kind.ToString() ?? "null";

            await using var check = await factory.CreateDbContextAsync(path);
            var names = await check.Columns.AsNoTracking().OrderBy(c => c.Id).Select(c => c.Name).ToListAsync();
            result += $" names=[{string.Join("|", names)}]";
        }

        foreach (var file in new[] { path, $"{path}-wal", $"{path}-shm" })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        return $"{label}: {result}";
    }

    [Fact]
    public async Task Probe()
    {
        using var provider = BuildProvider();

        output.WriteLine(await TrimWithRegions(provider, "rows only", captureRows: true, captureColumns: false));
        output.WriteLine(await TrimWithRegions(provider, "columns only", captureRows: false, captureColumns: true));
        output.WriteLine(await TrimWithRegions(provider, "both", captureRows: true, captureColumns: true));
    }
}

