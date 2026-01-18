using System.ComponentModel.DataAnnotations.Schema;
using ModelFlow.DataVirtualization.DataManagement;

namespace TableConverter.Utilities.Database.Models;

public class EntityBaseWithAutoSynchronize<T> : EntityBase<T>, IAutoSynchronize
{
    #region IAutoSynchronize Implementation

    [NotMapped]
    public bool CanSave { get; set; } = true;
    
    [NotMapped]
    public IDataManager? DataManager { get; set; }

    [NotMapped]
    public bool IsManaged { get; set; } = false;

    #endregion
}