using FastMember;

namespace TableConverter.Utilities.Extensions;

public static class ObjectExtensions
{
    public static TDestination MapTo<TDestination>(this TDestination to, object source, params string[] ignoredTargetProperties)
    {
        var sourceAccessor = ObjectAccessor.Create(source);
        var toAccessor = ObjectAccessor.Create(to);

        var commonProperties = TypeAccessor.Create(typeof(TDestination))
            .GetMembers()
            .Where(m => m.CanWrite)
            .Select(m => m.Name)
            .Except(ignoredTargetProperties)
            .Intersect(TypeAccessor.Create(source.GetType())
                .GetMembers()
                .Select(m => m.Name));

        foreach (var name in commonProperties)
        {
            toAccessor[name] = sourceAccessor[name];
        }

        return to;
    }
}