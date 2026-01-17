using FastMember;

namespace TableConverter.Utilities.Extensions;

public static class ObjectExtensions
{
    public static TDestination MapTo<TDestination, TSource>(this TDestination to, TSource source, params string[] ignoredTargetProperties)
    {
        var sourceAccessor = ObjectAccessor.Create(source);
        var toAccessor = ObjectAccessor.Create(to);

        var commonProperties = TypeAccessor.Create(typeof(TDestination))
            .GetMembers()
            .Where(m => m.CanWrite)
            .Select(m => m.Name)
            .Except(ignoredTargetProperties)
            .Intersect(TypeAccessor.Create(typeof(TSource)).GetMembers().Select(m => m.Name));

        foreach (var name in commonProperties)
        {
            toAccessor[name] = sourceAccessor[name];
        }

        return to;
    }
    
    public static bool CompareTo<TSource, TDestination>(this TDestination to, TSource source, params string[] ignoredProperties)
    {
        var sourceAccessor = ObjectAccessor.Create(source);
        var toAccessor = ObjectAccessor.Create(to);

        var commonProperties = TypeAccessor.Create(typeof(TDestination))
            .GetMembers()
            .Where(m => m.CanWrite)
            .Select(m => m.Name)
            .Except(ignoredProperties)
            .Intersect(TypeAccessor.Create(typeof(TSource)).GetMembers().Select(m => m.Name));

        foreach (var name in commonProperties)
        {
            var sourceValue = sourceAccessor[name];
            var toValue = toAccessor[name];

            if (!Equals(sourceValue, toValue))
            {
                return false;
            }
        }

        return true;
    }
}