namespace TableConverter.Utilities.Database;

/// <summary>
///     How much of a table there is: the two counts that change when the whole of it is rotated.
/// </summary>
/// <param name="RowCount">The number of rows the table holds.</param>
/// <param name="ColumnCount">The number of columns the table holds.</param>
public readonly record struct TableShape(int RowCount, int ColumnCount);

