using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;

namespace TableConverter.ViewModels.Workspaces;

public class DataGenerationWorkspaceEditorViewModel : BaseWorkspaceEditorViewModel
{
    #region Constructors
    
    public DataGenerationWorkspaceEditorViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider , "Data Generation", "DataAddIcon", 3)
    {
    }
    
    #endregion

    #region Methods

    public override IPaneDocument CreateNewDocumentInstance()
    {
        return _serviceProvider.GetRequiredService<DataGenerationSchemaViewModel>();
    }

    protected override Task<IPaneDocument> CreateDefaultDocumentInstanceAsync()
    {
        if (CreateNewDocumentInstance() is not DataGenerationSchemaViewModel schema)
        {
            throw new InvalidOperationException("Failed to create default DataGenerationSchemaViewModel instance.");
        }

        schema.Title = "Schema-Designer".GetUniqueString();

        return Task.FromResult<IPaneDocument>(schema);
    }

    #endregion
}