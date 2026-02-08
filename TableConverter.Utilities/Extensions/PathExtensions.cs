namespace TableConverter.Utilities.Extensions;

public static class PathExtensions
{
    public static void EnsureDirectoryExists(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }
        
        var fullPath = Path.GetFullPath(path);
        
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }
}