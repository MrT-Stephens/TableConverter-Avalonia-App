using System;
using System.Linq;
using System.Reflection;
using TableConverter.Contracts;
using TableConverter.DataGeneration;
using TableConverter.DataGeneration.Modules;
using TableConverter.Services.DataGenerationAttributedModules;
using TableConverter.Utilities.Models;

namespace TableConverter.Services;

public class FakerWithAttributedModules(string localeType = "en", int? seed = null) : FakerBase(localeType, seed)
{
    public override PersonModule Person => new PersonAttributedModule(this, Locale, Randomizer);
    public override PhoneModule Phone => new PhoneAttributedModule(this, Locale, Randomizer);
    public override LocationModule Location => new LocationAttributedModule(this, Locale, Randomizer);
    public override InternetModule Internet => new InternetAttributedModule(this, Locale, Randomizer);
    public override WordModule Word => new WordAttributedModule(this, Locale, Randomizer);
    public override LoremModule Lorem => new LoremAttributedModule(this, Locale, Randomizer);
    public override SystemModule System => new SystemAttributedModule(this, Locale, Randomizer);
    public override ScienceModule Science => new ScienceAttributedModule(this, Locale, Randomizer);
    public override MusicModule Music => new MusicAttributedModule(this, Locale, Randomizer);
    public override NumberModule Number => new NumberAttributedModule(this, Locale, Randomizer);
    public override ImageModule Image => new ImageAttributedModule(this, Locale, Randomizer);
    public override ColorModule Color => new ColorAttributedModule(this, Locale, Randomizer);
    public override VehicleModule Vehicle => new VehicleAttributedModule(this, Locale, Randomizer);
    public override CompanyModule Company => new CompanyAttributedModule(this, Locale, Randomizer);
    public override CommerceModule Commerce => new CommerceAttributedModule(this, Locale, Randomizer);
    public override FoodModule Food => new FoodAttributedModule(this, Locale, Randomizer);
    public override DateTimeModule DateTime => new DateTimeAttributedModule(this, Locale, Randomizer);

    public static KeyedFakerBuilder Create(FakerWithAttributedModules? faker = null)
    {
        faker ??= new FakerWithAttributedModules();

        return new KeyedFakerBuilder(faker);
    }

    public sealed class KeyedFakerBuilder(FakerWithAttributedModules faker)
        : FakerBuilderBase<FakerWithAttributedModules>(faker)
    {
        /// <summary>
        ///     Adds a "keyed" column where the column value is generated using a method or property based on the key.
        ///     The key can reference properties or methods, e.g., "Person.FirstName".
        /// </summary>
        /// <param name="columnName">The name of the column to add, or blank to have one made up.</param>
        /// <param name="key">The key for referencing a method or property (e.g., "Person.FirstName").</param>
        /// <param name="parameters">An array of parameters to pass to the method or property referenced by the key.</param>
        /// <param name="blanksPercentage">The percentage (0-100) of rows that should have a blank value in this column.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <remarks>
        ///     The column is declared with the kind of value the method behind the key produces, which is
        ///     the kind that method names in its <see cref="DataGenerationModuleMethodAttribute" />. That
        ///     is what keeps a generated table from being nothing but text.
        /// </remarks>
        public KeyedFakerBuilder AddKeyed(string columnName, string key, object[] parameters,
            int blanksPercentage = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            AddColumn(columnName, ResolveDataType(key), GenerateValue, blanksPercentage);

            return this;

            // Generate one value, reading it off the module the key names.
            string GenerateValue(FakerWithAttributedModules faker)
            {

                // Split the key into parts (e.g., "Person.FirstName" => ["Person", "FirstName"])
                var parts = key.Split('.');

                // Access the correct module (e.g., "Person" -> faker.Person)
                var module = GetModule(faker, parts[0]);

                // Resolve the property or method dynamically
                var result = GetPropertyOrMethod(module, parts, 1, parameters);

                return result.ToString() ?? string.Empty;
            }

            // Helper method to resolve the module dynamically based on the key part (e.g., "Person")
            object GetModule(FakerWithAttributedModules faker, string moduleName)
            {
                var property =
                    typeof(FakerWithAttributedModules).GetProperty(moduleName,
                        BindingFlags.Public | BindingFlags.Instance);

                return property?.GetValue(faker)
                       ?? throw new InvalidOperationException($"Module '{moduleName}' not found.");
            }

            // Helper method to resolve the property or method dynamically from the module
            object GetPropertyOrMethod(object module, string[] parts, int index, object[] methodParameters)
            {
                // If we reach the end of the parts array, return the current module
                if (index == parts.Length)
                    return module;

                var currentPart = parts[index];

                // First, check for a method with the name `currentPart`
                var method = module.GetType().GetMethod(currentPart, BindingFlags.Public | BindingFlags.Instance);

                if (method is not null)
                    // Invoke the method with parameters
                    return method.Invoke(module, methodParameters)
                           ?? throw new InvalidOperationException($"Method '{currentPart}' returned null.");

                // Next, check for a property with the name `currentPart`
                var property = module.GetType().GetProperty(currentPart, BindingFlags.Public | BindingFlags.Instance);

                if (property is not null)
                    // Return the value of the property and continue recursion
                    return GetPropertyOrMethod(
                        property.GetValue(module) ??
                        throw new InvalidOperationException($"Property '{currentPart}' returned null."),
                        parts, index + 1, methodParameters);

                throw new InvalidOperationException($"Neither method nor property '{currentPart}' found on module.");
            }

            // Helper method to resolve the kind of value the method named by the key produces.
            static ColumnDataType ResolveDataType(string key)
            {
                var parts = key.Split('.');

                // A key has to name a module and something on it, so anything shorter names nothing.
                if (parts.Length < 2)
                    return ColumnDataType.Text;

                var module = typeof(FakerWithAttributedModules)
                    .GetProperty(parts[0], BindingFlags.Public | BindingFlags.Instance);

                if (module is null)
                    return ColumnDataType.Text;

                var type = module.PropertyType;

                // Walk the same path the value is read along, but over the types rather than the objects.
                for (var index = 1; index < parts.Length; index++)
                {
                    var method = type.GetMethod(parts[index], BindingFlags.Public | BindingFlags.Instance);

                    // A method ends the path, and it is the one that says what kind of value it produces.
                    if (method is not null)
                        return method.GetCustomAttribute<DataGenerationModuleMethodAttribute>()?.DataType
                               ?? ColumnDataType.Text;

                    var property = type.GetProperty(parts[index], BindingFlags.Public | BindingFlags.Instance);

                    if (property is null)
                        return ColumnDataType.Text;

                    type = property.PropertyType;
                }

                return ColumnDataType.Text;
            }
        }
    }
}