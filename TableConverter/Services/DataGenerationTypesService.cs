using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TableConverter.DataModels;

namespace TableConverter.Services;

public class DataGenerationTypesService
{
    private static readonly IReadOnlyList<DataGenerationType> DataGenerationTypes = LoadDataGenerationTypes();
    
    private static readonly FakerWithAttributedModules Faker = new();

    public IReadOnlyList<DataGenerationType> Types => DataGenerationTypes;

    private static IReadOnlyList<DataGenerationType> LoadDataGenerationTypes()
    {
        List<DataGenerationType> dataGenerationTypes = [];

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                     .Where(t => t.GetCustomAttribute<DataGenerationModuleAttribute>() is not null))
        {
            List<DataGenerationMethod> methods = [];

            foreach (var method in type.GetMethods()
                         .Where(m => m.GetCustomAttribute<DataGenerationModuleMethodAttribute>() is not null))
            {
                var parameters = method.GetParameters().Select(
                    p => new DataGenerationMethodParameter
                    (
                        p.Name ?? "",
                        p.ParameterType,
                        p.HasDefaultValue ? p.DefaultValue : null
                    )).ToList();

                if (method.GetCustomAttribute<DataGenerationModuleMethodAttribute>() is not { } methodAttribute)
                    throw new Exception("Method attribute not found.");

                var methodViewModel = new DataGenerationMethod(
                    methodAttribute.Name,
                    methodAttribute.Description,
                    parameters,
                    args => { }
                );

                methods.Add(methodViewModel);
            }

            if (type.GetCustomAttribute<DataGenerationModuleAttribute>() is not { } moduleAttribute)
                throw new Exception("Module attribute not found.");

            dataGenerationTypes.Add(new DataGenerationType(
                moduleAttribute.Name,
                moduleAttribute.Description,
                methods
            ));
        }

        return dataGenerationTypes;
    }
}