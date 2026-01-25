using System.Security.Cryptography;
using System.Text;

namespace TableConverter.Utilities;

public static class GuidUtility
{
    public static readonly Guid DnsNamespace = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    public static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    public static readonly Guid OidNamespace = new("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

    public static readonly Guid X500Namespace = new("6ba7b814-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// Creates a name-based (version 5) GUID using SHA-1
    /// </summary>
    public static Guid Create(Guid namespaceId, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        // Convert namespace UUID to network order (big-endian)
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // Compute SHA-1 hash of namespace + name
        var nameBytes = Encoding.UTF8.GetBytes(name);

        using var sha1 = SHA1.Create();
        sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
        sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);

        var hash = sha1.Hash!;
        
        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);

        // Set version to 5
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));

        // Set variant to RFC 4122
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        // Convert back to little-endian
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        Swap(0, 3);
        Swap(1, 2);
        Swap(4, 5);
        Swap(6, 7);
        
        return;

        void Swap(int a, int b)
        {
            (guid[a], guid[b]) = (guid[b], guid[a]);
        }
    }
}
