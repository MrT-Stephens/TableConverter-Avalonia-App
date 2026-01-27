using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;

namespace TableConverter.Views.Controls.Spreadsheet;

public partial class SpreadsheetGrid
{
    private readonly double _uiRowHeaderWidth = 50;
    public static readonly double RowHeightInExcelToUiFactor = 96.0 / 72.0;

    // The default value here is 16.5 instead of 15
    public static readonly double DefaultRowHeightInExcel = 16.5;
    private readonly double _minRowHeight = DefaultRowHeightInExcel *
                                            RowHeightInExcelToUiFactor;

    private List<RowSplitter> _RowSplitters = [];
    private bool _IsRowSplitterMoving;
    private Point _RowSplitterStartPoint; //need to be recorded

    //Offset for pixel-level scrolling
    private double _PixelOffsetY = 0;
    private int _VisualRowStartId = -1;
    private int _VisualRowEndId = -1;

    // When scrolling with the vertical scroll bar
    public void OnVerticalScrollBarScroll(double newValue0100)
    {
        _log.Debug($"enter OnVerticalScrollBarScroll()");

        if (newValue0100 is < 0 or > 100)
        {
            return;
        }

        // Use pixel-level scrolling
        _PixelOffsetY = newValue0100 * GetTotalRowHeight() / 100;
        ViewportOffsetY = _PixelOffsetY; // maintain compatibility

        _log.Debug($"OnVerticalScrollBarScroll(): newValue_0_100={newValue0100} pixelOffsetY={_PixelOffsetY}");

        CalcVisualRowRegion();
        UpdateRowSplitter();
        UpdateClearHighlightRange();
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        _log.Debug($"enter OnPointerWheelChanged()");

        base.OnPointerWheelChanged(e);

        // Use pixel level scrolling
        double pixelScrollAmount = 40; //The number of pixels each time the wheel scrolls, adjustable

        if (e.Delta.Y < 0){
            _PixelOffsetY += pixelScrollAmount;
            if (_PixelOffsetY > GetTotalRowHeight() - Bounds.Height + UiColumnHeaderHeight)
            {
                _PixelOffsetY = GetTotalRowHeight() - Bounds.Height + UiColumnHeaderHeight;
            }
        }
        else
        {
            _PixelOffsetY -= pixelScrollAmount;
            
            if (_PixelOffsetY < 0)
            {
                _PixelOffsetY = 0;
            }
        }

        CalcVisualRowRegion();
        UpdateRowSplitter();
        UpdateClearHighlightRange();
        InvalidateVisual();
    }

    private void CalcVisualRowRegion()
    {
        _log.Debug($"enter CalcVisualRowRegion(): pixelOffsetY={_PixelOffsetY}, Bounds.Height={Bounds.Height}");

        ViewportOffsetY = _PixelOffsetY; // maintain compatibility
        int totalRows = SheetData.GetValidRowCount();
        if (totalRows == 0)
        {
            _VisualRowStartId = -1;
            _VisualRowEndId = -1;
            return;
        }

        double viewportTop = _PixelOffsetY;
        double viewportBottom = _PixelOffsetY + Bounds.Height - UiColumnHeaderHeight;

        // Use binary search to find the viewport starting row
        _VisualRowStartId = FindFirstVisibleRow(totalRows, viewportTop);

        // If no visible rows are found
        if (_VisualRowStartId == -1)
        {
            _VisualRowEndId = -1;
            return;
        }

        // Search the end line sequentially
        _VisualRowEndId = _VisualRowStartId;
        double currentTop = _RowHeightAccumulator.GetAccumulated(_VisualRowStartId);

        for (int rowId = _VisualRowStartId; rowId < totalRows; rowId++)
        {
            double rowHeight = SheetData.GetRowHeightInUi((uint)rowId);
            double rowBottom = currentTop + rowHeight;

            if (currentTop >= viewportBottom)
            {
                _VisualRowEndId = rowId - 1;
                break;
            }

            if (rowBottom > viewportBottom)
            {
                _VisualRowEndId = rowId;
                break;
            }

            if (rowId == totalRows - 1){
                _VisualRowEndId = rowId;
            }

            currentTop = rowBottom;
        }

        _log.Debug(
            $"CalcVisualRowRegion(): Bounds={Bounds} Row={_VisualRowStartId}->{_VisualRowEndId} viewportTop={viewportTop} viewportBottom={viewportBottom} totalRows={SheetData.GetValidRowCount()}");
    }

    private int FindFirstVisibleRow(int totalRows, double viewportTop)
    {
        int low = 0;
        int high = totalRows - 1;
        int candidate = -1;

        // Dichotomy
        while (low <= high)
        {
            int mid = (low + high) / 2;
            double rowTop = _RowHeightAccumulator.GetAccumulated(mid);
            double rowBottom = rowTop + SheetData.GetRowHeightInUi((uint)mid);

            if (rowBottom <= viewportTop)
            {
                low = mid + 1;
            }
            else
            {
                candidate = mid;
                high = mid - 1;
            }
        }

        return candidate;
    }

    private void UpdateRowSplitter()
    {
        _log.Debug($"enter UpdateRowSplitter()");

        // First clear all rowsplitter objects
        foreach (var oldRowSplitter in _RowSplitters){
            this.Children.Remove(oldRowSplitter);
        }

        // Regenerate all rowsplitter objects and assign values
        List<RowSplitter> newRowSplitters = new List<RowSplitter>();

        // Calculate the position of the line separator, taking into account pixel-level scrolling
        var accHeight = UiColumnHeaderHeight;
        for (int rowId = _VisualRowStartId; rowId < _VisualRowEndId; rowId++)
        {
            accHeight += SheetData.GetRowHeightInUi((uint)rowId);
            var rowSplitter = new RowSplitter(rowId, _uiRowHeaderWidth);

            newRowSplitters.Add(rowSplitter);
            // CanvasX.InvalidateMeasureOnChildrenChanged()
            Children.Add(rowSplitter);

            // Set the correct position
            double splitterPosition = CalculateRowPositionY(rowId + 1);
            SetTop(rowSplitter, splitterPosition);

            // Mouse click trilogy: refer to reogrid.SheetTabControl rightThumb code
            rowSplitter.PointerPressed += (_, e) => 
            {
                _log.Debug($"enter PointerPressed.");

                if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    return;
                }

                e.Handled = true;
                e.Pointer.Capture(rowSplitter);
                _IsRowSplitterMoving = true;

                //Coordinates relative to Canvas, this=canvas
                _RowSplitterStartPoint = e.GetPosition(this);
                // Console.WriteLine($"spiltterStartPoint={_rowSpiltterStartPoint}");
                base.OnPointerPressed(e);
            };

            rowSplitter.PointerMoved += (_, _) => 
            {
                _log.Debug("enter PointerMoved.");

                if (_IsRowSplitterMoving)
                {
                    //Console.WriteLine("rowSplitter.PointerMoved()");
                }
            };

            rowSplitter.PointerReleased += (s, e) => {
                _log.Debug($"enter PointerReleased.");

                if (s is not RowSplitter splitter || !_IsRowSplitterMoving)
                {
                    return;
                }

                e.Pointer.Capture(null);
                _IsRowSplitterMoving = false;

                //Update row height
                var curPoint = e.GetPosition(this);
                // Console.WriteLine($"curPoint={curPoint}");

                // may be a positive number or a negative number
                var delta = curPoint.Y - _RowSplitterStartPoint.Y;

                var newHeight = SheetData.GetRowHeightInUi((uint)splitter.RowId) + delta;

                // Cannot be lower than the minimum height
                if (newHeight <= _minRowHeight){
                    newHeight = _minRowHeight;
                }

                // Console.WriteLine($"rowid={((RowSplitter)s).RowID} newHeight={newHeight}");
                SheetData.SetRowHeightInUi((uint)splitter.RowId, (float)newHeight);
                _RowHeightAccumulator.UpdateSize(splitter.RowId, newHeight); // update accumulator
                _TotalRowHeight = GetTotalRowHeight(); // Update the total height of the cache

                base.OnPointerReleased(e);

                // You need to refresh after the mouse is released, otherwise these objects will still be in their original positions.
                CalcVisualRowRegion();
                UpdateRowSplitter();
                //InvalidateArrange();
            };
        }

        // replace
        _RowSplitters = newRowSplitters;
    }

    //Calculate the row's display position on the screen, taking scroll offset into account
    private double CalculateRowPositionY(int rowId)
    {
        if (rowId < 0)
        {
            return UiColumnHeaderHeight;
        }

        // Use the accumulator to get the cumulative altitude
        var totalHeightBefore = _RowHeightAccumulator.GetAccumulated(rowId);

        //apply scroll offset
        return UiColumnHeaderHeight + totalHeightBefore - _PixelOffsetY;
    }
}