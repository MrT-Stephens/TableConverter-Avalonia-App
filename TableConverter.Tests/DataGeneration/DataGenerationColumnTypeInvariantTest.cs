using System.Reflection;
using System.Text;
using TableConverter.Contracts;
using TableConverter.Services;
using TableConverter.Utilities.Models;

namespace TableConverter.Tests.DataGeneration;

/// <summary>
///     Checks that a method which names the kind of value it produces really does produce that kind of
///     value.
/// </summary>
/// <remarks>
///     <para>
///         A generated table is declared column by column from these names rather than having its types
///         read off its values, so a name that does not match what the method produces is the one way a
///         generated table can be wrong about itself. Every other table settles its own types by reading
///         its values, and cannot get this wrong.
///     </para>
///     <para>
///         Each attributed method is therefore run with the arguments it declares and the value it hands
///         back is read the way the application will read it: through
///         <see cref="ColumnDataTypeExtensions.IsValidValue" />, which is what decides whether the value
///         is shown as its column's type or as the raw text it was written as.
///     </para>
/// </remarks>
public class DataGenerationColumnTypeInvariantTest
{
    /// <summary>
    ///     The seed the methods are run with, so a run that reports a problem can be repeated.
    /// </summary>
    private const int Seed = 20260927;


    [Fact]
    public void Every_Attributed_Method_Produces_A_Value_That_Reads_As_The_Type_It_Declares()
    {
        var problems = new StringBuilder();
        var checkedMethods = 0;

        foreach (var type in ModuleTypes())
        {
            var moduleName = type.GetCustomAttribute<DataGenerationModuleAttribute>()!.Name;
            var faker = new FakerWithAttributedModules("en", Seed);
            var module = Activator.CreateInstance(type, faker, faker.Locale, faker.Randomizer)!;

            foreach (var method in AttributedMethods(type))
            {
                var attribute = method.GetCustomAttribute<DataGenerationModuleMethodAttribute>()!;
                var reported = $"{moduleName}.{attribute.Name} ({method.Name})";

                object? value;

                try
                {
                    value = method.Invoke(module, Arguments(method));
                }
                catch (TargetInvocationException exception)
                {
                    // A method that will not run cannot be shown to produce the kind of value it names,
                    // so it counts against the invariant rather than being quietly passed over.
                    problems.AppendLine(
                        $"{reported}: threw {exception.InnerException?.GetType().Name} - {exception.InnerException?.Message}");
                    continue;
                }

                checkedMethods++;

                var text = value?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(text))
                {
                    problems.AppendLine($"{reported}: declares {attribute.DataType} but produced nothing");
                }
                else if (!attribute.DataType.IsValidValue(text))
                {
                    problems.AppendLine(
                        $"{reported}: declares {attribute.DataType} but produced '{text}', which does not read as it");
                }
            }
        }

        Assert.True(checkedMethods > 0, "No attributed methods were found to check.");
        Assert.Empty(problems.ToString());
    }

    /// <summary>
    ///     The modules the application lists, which is every type that names itself one.
    /// </summary>
    private static IEnumerable<Type> ModuleTypes()
    {
        return typeof(FakerWithAttributedModules).Assembly
            .GetTypes()
            .Where(type => type.GetCustomAttribute<DataGenerationModuleAttribute>() is not null)
            .OrderBy(type => type.Name);
    }

    /// <summary>
    ///     The methods <paramref name="type" /> offers as a way of generating a value.
    /// </summary>
    private static IEnumerable<MethodInfo> AttributedMethods(Type type)
    {
        return type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<DataGenerationModuleMethodAttribute>() is not null)
            .OrderBy(method => method.Name);
    }

    /// <summary>
    ///     The arguments to run <paramref name="method" /> with: what it declares for each parameter, so
    ///     that what is checked is what the method offers out of the box.
    /// </summary>
    private static object?[] Arguments(MethodInfo method)
    {
        return method.GetParameters()
            .Select(parameter => parameter.HasDefaultValue
                ? parameter.DefaultValue
                : Fallback(parameter.ParameterType))
            .ToArray();
    }

    /// <summary>
    ///     What a parameter with nothing declared for it is given, which is all that its own type can say.
    /// </summary>
    private static object? Fallback(Type type)
    {
        if (type == typeof(string))
        {
            return string.Empty;
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
