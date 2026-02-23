using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelFlow.DataVirtualization.DataManagement;

namespace TableConverter.Services.DataSources;

public class LoggingDataSourceCallbacks(ILogger<LoggingDataSourceCallbacks> logger) : IDataSourceCallbacks
{
    #region IDataSourceCallbacks Implementation
    
    public Task<bool> OnBeforeCreateOperation(object viewmodel)
    {
        logger.LogInformation(
            new EventId(0, nameof(OnBeforeCreateOperation)), 
            "Data Source Callback: Create for '{0}'", 
            viewmodel.GetType().Name);
        
        return Task.FromResult(true);
    }

    public Task<bool> OnBeforeUpdateOperation(object viewmodel)
    {
        logger.LogInformation(
            new EventId(1, nameof(OnBeforeUpdateOperation)),
            "Data Source Callback: Update for '{0}'", 
            viewmodel.GetType().Name);
        
        return Task.FromResult(true);
    }

    public Task<bool> OnBeforeDeleteOperation(object viewmodel)
    {
        logger.LogInformation(
            new EventId(2, nameof(OnBeforeDeleteOperation)), 
            "Data Source Callback: Delete for '{0}'", 
            viewmodel.GetType().Name);
        
        return Task.FromResult(true);
    }

    public Task OnCreateOperationCompleted(object viewmodel, bool success)
    {
        logger.LogInformation(
            new EventId(3, nameof(OnCreateOperationCompleted)),
            "Data Source Callback: Create Completed for '{0}' with success '{1}'",
            viewmodel.GetType().Name, success);
        
        return Task.CompletedTask;
    }

    public Task OnUpdateOperationCompleted(object viewmodel, bool success)
    {
        logger.LogInformation(
            new EventId(4, nameof(OnUpdateOperationCompleted)),
            "Data Source Callback: Update Completed for '{0}' with success '{1}'",
            viewmodel.GetType().Name, success);
        
        return Task.CompletedTask;
    }

    public Task OnDeleteOperationCompleted(object viewmodel, bool success)
    {
        logger.LogInformation(
            new EventId(5, nameof(OnDeleteOperationCompleted)),
            "Data Source Callback: Delete Completed for '{0}' with success '{1}'",
            viewmodel.GetType().Name, success);
        
        return Task.CompletedTask;
    }

    public Task OnCreateException(object viewmodel, Exception exception)
    {
        logger.LogError(
            new EventId(6, nameof(OnCreateException)),
            exception,
            "Data Source Callback: Create Exception for '{0}'",
            viewmodel.GetType().Name);
        
        return Task.CompletedTask;
    }

    public Task OnUpdateException(object viewmodel, Exception exception)
    {
        logger.LogError(
            new EventId(7, nameof(OnUpdateException)),
            exception,
            "Data Source Callback: Update Exception for '{0}'",
            viewmodel.GetType().Name);
        
        return Task.CompletedTask;
    }

    public Task OnDeleteException(object viewmodel, Exception exception)
    {
        logger.LogError(
            new EventId(8, nameof(OnDeleteException)),
            exception,
            "Data Source Callback: Delete Exception for '{0}'",
            viewmodel.GetType().Name);
        
        return Task.CompletedTask;
    }
    
    #endregion
}