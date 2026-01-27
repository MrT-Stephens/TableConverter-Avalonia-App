namespace TableConverter.Views.Controls.Spreadsheet;

public interface ISheetData
{
    string? SheetName{ get; set; }
    
    string? GetCellText(int cellRowId, int cellColId);
    
    int GetValidRowCount();
    
    double GetRowHeightInUi(uint u);
    
    void SetRowHeightInUi(uint rowId, float newHeight);
    
    int GetValidColCount();
    
    double GetColWidthInUi(uint u);
    
    void SetColWidthInUi(uint colId, float newWidth);
}