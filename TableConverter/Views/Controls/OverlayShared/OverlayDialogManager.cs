using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TableConverter.Views.Controls.OverlayShared;

internal record struct HostKey(string? Id, int? Hash);

internal static class OverlayDialogManager
{
    private static readonly ConcurrentDictionary<HostKey, OverlayDialogHost> Hosts = new();

    public static void RegisterHost(OverlayDialogHost host, string? id, int? hash)
    {
        Debug.WriteLine("Count: " + Hosts.Count);
        Hosts.TryAdd(new HostKey(id, hash), host);
    }

    public static void UnregisterHost(string? id, int? hash)
    {
        Hosts.TryRemove(new HostKey(id, hash), out _);
    }

    public static OverlayDialogHost? GetHost(string? id, int? hash)
    {
        HostKey? key = hash is null 
            ? Hosts.Keys.Where(k => k.Id == id).ToArray().FirstOrDefault() 
            : Hosts.Keys.FirstOrDefault(k => k.Id == id && k.Hash == hash);

        return key is null 
            ? null 
            : Hosts.GetValueOrDefault(key.Value);
    }
}
