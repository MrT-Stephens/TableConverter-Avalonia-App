using System.Collections.ObjectModel;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Models;

namespace TableConverter.ViewModels.Tools;

/// <summary>
///     What one column of a table holds that does not read as its type, as the validation panel lists it.
/// </summary>
public sealed class ValidationColumnViewModel
{
    public ValidationColumnViewModel(
        ColumnValidationSummary summary,
        ObservableCollection<ValidationCellViewModel> cells)
    {
        ColumnId = summary.ColumnId;
        Name = summary.ColumnName;
        DataType = summary.DataType;
        InvalidCount = summary.InvalidCount;
        Cells = cells;
    }

    /// <summary>The id of the column these values belong to.</summary>
    public int ColumnId { get; }

    /// <summary>The column's name.</summary>
    public string Name { get; }

    /// <summary>The type the column was given, which is what its values were expected to read as.</summary>
    public ColumnDataType DataType { get; }

    /// <summary>How many of the column's values do not read as its type.</summary>
    public int InvalidCount { get; }

    /// <summary>The faulty values that are listed, up to the number the report was asked to describe.</summary>
    public ObservableCollection<ValidationCellViewModel> Cells { get; }

    /// <summary>The column's heading, which is its name and the type its values were expected to read as.</summary>
    public string Header => $"{Name} · {DataType}";

    /// <summary>How many of the column's values do not read as its type, phrased as a sentence fragment.</summary>
    public string CountLabel => InvalidCount == 1
        ? "1 value doesn't read as this type"
        : $"{InvalidCount:N0} values don't read as this type";

    /// <summary>Whether more values are faulty than are listed.</summary>
    public bool IsTruncated => Cells.Count < InvalidCount;

    /// <summary>How many faulty values are counted but not listed.</summary>
    public string TruncatedLabel => IsTruncated ? $"+{InvalidCount - Cells.Count:N0} more" : string.Empty;
}

