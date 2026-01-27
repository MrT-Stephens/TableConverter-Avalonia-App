using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace TableConverter.Views.Controls.Spreadsheet;

public class ColumnSplitter : Border
{
    public int ColId { get; }

    public ColumnSplitter(int colId, double colHeaderHeight)
    {
        Width = 5;
        Height = colHeaderHeight;
        Cursor = new Cursor(StandardCursorType.SizeWestEast);
        ColId = colId;
        
#if DEBUG
        Background = Brushes.Transparent;
        ToolTip.SetTip(this, $"colID={colId}");
#else
        Background = Brushes.Transparent;
#endif
    }
}