using System;
using System.Threading.Tasks;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Services.DataSources.Base;

public abstract class DataSourceFromPath<TModel> : DataSource<TModel> 
    where TModel : class 
{
    #region Properties

    protected readonly ITableStoreDbContextFactory _databaseContextFactory;

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
        ITableStoreDbContextFactory factory, 
        int pageSize, 
        int maxPages, 
        bool autoSync = true) 
        : base(pageSize, maxPages, autoSync)
    {
        _databaseContextFactory = factory;
    }
    
    #endregion

    #region Methods

    protected Task<TableStoreDbContext> CreateDbAsync()
    {
        return _databaseContextFactory.CreateDbContextAsync(Path);
    }

    #endregion
}