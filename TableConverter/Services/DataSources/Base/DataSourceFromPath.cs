using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities;

namespace TableConverter.Services.DataSources.Base;

public abstract class DataSourceFromPath<TModel, TContext> : DataSource<TModel> 
    where TModel : class 
    where TContext : DbContext
{
    #region Properties

    protected readonly Utilities.Database.Interfaces.IDatabaseContextFactory<TContext> _databaseContextFactory;

    private Guid? _SourceId = null;
    public Guid? SourceId
    {
        get => _SourceId;
        set
        {
            if (_SourceId == value) 
                return;
            
            _SourceId = value;
            OnPropertyChanged();
        }
    }

    private string _Path = string.Empty;
    public string Path
    {
        get => _Path;
        set
        {
            if (_Path == value) 
                return;
            
            _Path = value;
            SourceId = string.IsNullOrEmpty(value) 
                ? null 
                : GuidUtility.Create(GuidUtility.UrlNamespace, value);
            Invalidate();
            OnPropertyChanged();
        }
    }

    #endregion
    
    #region Constructors
    
    protected DataSourceFromPath(
        Utilities.Database.Interfaces.IDatabaseContextFactory<TContext> factory, 
        int pageSize, 
        int maxPages, 
        bool autoSync = true) 
        : base(pageSize, maxPages, autoSync)
    {
        _databaseContextFactory = factory;
    }
    
    #endregion

    #region Methods

    protected Task<TContext> CreateDbAsync()
    {
        return _databaseContextFactory.CreateAsync(Path);
    }

    #endregion
}