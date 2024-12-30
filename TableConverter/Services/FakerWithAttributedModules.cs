using System;
using System.Linq;
using System.Reflection;
using TableConverter.DataGeneration;
using TableConverter.DataGeneration.Modules;
using TableConverter.Services.DataGenerationAttributedModules;

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
        /// <param name="columnName">The name of the column to add.</param>
        /// <param name="key">The key for referencing a method or property (e.g., "Person.FirstName").</param>
        /// <param name="parameters">An array of parameters to pass to the method or property referenced by the key.</param>
        /// <param name="blanksPercentage">The percentage (0-100) of rows that should have a blank value in this column.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public KeyedFakerBuilder AddKeyed(string columnName, string key, object[] parameters,
            int blanksPercentage = 0)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                columnName = $"Column-{_actions.SelectMany(kvp => kvp.Value).Count() + 1}";

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            if (blanksPercentage is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(blanksPercentage), blanksPercentage,
                    "Blanks percentage must be between 0 and 100.");

            // Add the adjusted generator to the dictionary for the column name
            if (!_actions.TryGetValue(columnName, out var generators))
            {
                generators = [];
                _actions[columnName] = generators;
            }

            generators.Add(AdjustedGenerator);

            return this;

            // Adjusted generator function
            string AdjustedGenerator(FakerWithAttributedModules faker)
            {
                // If the random number is below the blank percentage, return an empty value.
                if (faker.Randomizer.Number(0, 100) < blanksPercentage)
                    return string.Empty;

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
        }
    }
}