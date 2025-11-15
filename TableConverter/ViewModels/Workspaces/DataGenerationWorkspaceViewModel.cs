using System;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;

namespace TableConverter.ViewModels.Workspaces;

public class DataGenerationWorkspaceViewModel : BaseWorkspaceEditorViewModel
{
    #region Constructors
    
    public DataGenerationWorkspaceViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider , "Data Generation", "DataAddIcon", 3)
    {
    }
    
    #endregion

    #region Methods

    public override IPaneDocument CreateNewDocumentInstance()
    {
        return _serviceProvider.GetRequiredService<DataGenerationSchemaViewModel>();
    }

    protected override IPaneDocument CreateDefaultDocumentInstance()
    {
        if (CreateNewDocumentInstance() is not DataGenerationSchemaViewModel schema)
        {
            throw new InvalidOperationException("Failed to create default DataGenerationSchemaViewModel instance.");
        }

        schema.Title = "Schema-Designer".GetUniqueString();

        return schema;
    }

    #endregion
}