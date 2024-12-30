using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using TableConverter.DataModels;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public class DataGenerationTypesService
{
    private static readonly IReadOnlyList<DataGenerationType> DataGenerationTypes = LoadDataGenerationTypes();

    private static readonly FakerWithAttributedModules Faker = new();

    public IReadOnlyList<DataGenerationType> Types => DataGenerationTypes;

    public Task<DataGeneration.DataModels.TableData> GenerateData(DataGenerationFieldViewModel[] fields, int rowCount = 0)
    {
        var builder = FakerWithAttributedModules.Create(Faker);
        
        foreach (var field in fields)
        {
            builder.AddKeyed(
                field.Name,
                field.Key,
                field.Parameters.Select(param => param.Value).ToArray()!,
                field.BlankPercentage
            );
        }

        builder.WithRowCount(rowCount);
        
        return builder.BuildAsync();
    }

    private static IReadOnlyList<DataGenerationType> LoadDataGenerationTypes()
    {
        List<DataGenerationType> dataGenerationTypes = [];

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                     .Where(t => t.GetCustomAttribute<DataGenerationModuleAttribute>() is not null))
        {
            List<DataGenerationMethod> methods = [];

            if (type.GetCustomAttribute<DataGenerationModuleAttribute>() is not { } moduleAttribute)
                throw new Exception("Module attribute not found.");

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
                    $"{moduleAttribute.Name}.{methodAttribute.Name}".Replace(" ", string.Empty),
                    methodAttribute.Name,
                    methodAttribute.Description,
                    parameters
                );

                methods.Add(methodViewModel);
            }

            dataGenerationTypes.Add(new DataGenerationType(
                moduleAttribute.Name,
                moduleAttribute.Description,
                Application.Current?.Resources[moduleAttribute.IconResourceName] ??
                throw new KeyNotFoundException("Icon not found."),
                methods.OrderBy(x => x.Name).ToList()
            ));
        }

        return dataGenerationTypes.OrderBy(x => x.Name).ToList();
    }
}