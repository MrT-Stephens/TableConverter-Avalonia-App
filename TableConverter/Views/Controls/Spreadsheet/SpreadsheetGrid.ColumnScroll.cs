using System;
using System.Collections.Generic;
using Avalonia;

namespace TableConverter.Views.Controls.Spreadsheet;

public partial class SpreadsheetGrid
{
    // The height of the column header can accommodate a single line of letters.
    private static readonly double UiColumnHeaderHeight = 22;
    public static readonly double DefaultColWidthInExcel = 9;
    public static readonly double ColWidthInExcelToUiFactor = 6 * 96.0 / 72.0;

    private readonly double _minColWidth = DefaultColWidthInExcel *
                                          ColWidthInExcelToUiFactor;

    private List<ColumnSplitter> _ColSplitters = [];
    private bool _IsColSplitterMoving;
    private Point _ColSplitterStartPoint; //Need to be recorded
    private int _VisualColStartId = -1;
    private int _VisualColEndId = -1;

    public void OnHorizontalScrollBarScroll(double newValue0100)
    {
        if (newValue0100 is < 0 or > 100)
        {
            return;
        }

        // Calculate the coordinate value of the current viewPort. Note that the last line is not included here.
        ViewportOffsetX = newValue0100 * GetTotalColWidth() / 100;

        // Console.WriteLine($"newValue_0_100={newValue_0_100} ViewportOffsetY={ViewportOffsetY}");

        CalcVisualColRegion();
        UpdateColSplitter();
        UpdateClearHighlightRange();
        InvalidateVisual();
    }

    private void CalcVisualColRegion(){
        if (SheetData.GetValidRowCount() == 0 || SheetData.GetValidColCount() == 0)
        {
            _VisualColStartId = -1;
            _VisualColEndId = -1;
            return;
        }

        // Input: Starting point: ViewportOffsetX
        // Picture frame: The current drawing window bounds is viewport
        // Output: Calculate visibleRegion, which requires complete coverage of the frame

        double accWidth = 0;

        for (var i = 0; i < SheetData.GetValidColCount(); i++)
        {
            // Once the cumulative value is greater than or equal to X
            if (accWidth >= ViewportOffsetX)
            {
                _VisualColStartId = i;
                break;
            }
            else
            {
                accWidth += SheetData.GetColWidthInUi((uint)i);
            }

            // Tail boundary conditions
            if (i == SheetData.GetValidColCount() - 1)
            {
                _VisualColStartId = i;
            }
        }

        // Count from visualColStart to the end
        accWidth = 0;
        for (int i = _VisualColStartId; i < SheetData.GetValidColCount(); i++)
        {
            // I would rather have more, so the height is accumulated first
            accWidth += SheetData.GetColWidthInUi((uint)i);

            // Once the cumulative value is greater than or equal to
            if (accWidth >= Bounds.Width)
            {
                _VisualColEndId = i;
                break;
            }

            // Tail boundary conditions
            if (i == SheetData.GetValidColCount() - 1)
            {
                _VisualColEndId = i;
            }
        }

        //Console.WriteLine($"NewMethod() Bounds={Bounds} Col={visualColStart}->{visualColEnd}");
    }

    private void UpdateColSplitter()
    {
        // First clear all colsplitter objects
        foreach (var oldColSplitter in _ColSplitters){
            this.Children.Remove(oldColSplitter);
        }

        // Regenerate all colsplitter objects and assign values
        List<ColumnSplitter> newColSplitters = [];

        var accWidth = _uiRowHeaderWidth;
        
        for (int colId = _VisualColStartId; colId < _VisualColEndId; colId++)
        {
            accWidth += SheetData.GetColWidthInUi((uint)colId);
            var colSplitter = new ColumnSplitter(colId, UiColumnHeaderHeight);

            newColSplitters.Add(colSplitter);
            // CanvasX.InvalidateMeasureOnChildrenChanged()
            Children.Add(colSplitter);

            //Set the correct position
            SetLeft(colSplitter, accWidth);

            // Mouse click trilogy: refer to reogrid.SheetTabControl rightThumb code
            colSplitter.PointerPressed += (_, e) => 
            {
                if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    return;
                }

                e.Handled = true;
                e.Pointer.Capture(colSplitter);
                _IsColSplitterMoving = true;

                //Coordinates relative to Canvas, this=canvas
                _ColSplitterStartPoint = e.GetPosition(this);
                // Console.WriteLine($"spiltterStartPoint={_colSpiltterStartPoint}");
                base.OnPointerPressed(e);
            };

            colSplitter.PointerMoved += (_, _) => 
            {
                if (_IsColSplitterMoving)
                {
                    //Console.WriteLine("colSplitter.PointerMoved()");
                }
            };

            colSplitter.PointerReleased += (s, e) => 
            {
                if (!_IsColSplitterMoving)
                {
                    return;
                }

                e.Pointer.Capture(null);
                _IsColSplitterMoving = false;

                //Update col height
                var curPoint = e.GetPosition(this);
                // Console.WriteLine($"curPoint={curPoint}");

                // It may be a positive number or it may be a negative number
                var delta = curPoint.X - _ColSplitterStartPoint.X;

                var newWidth = SheetData.GetColWidthInUi((uint)((ColumnSplitter)s!).ColId) + delta;

                // Cannot be lower than the minimum width
                if (newWidth <= _minColWidth)
                {
                    newWidth = _minColWidth;
                }

                Console.WriteLine($"colid={((ColumnSplitter)s).ColId} newWidth={newWidth}");
                SheetData.SetColWidthInUi((uint)((ColumnSplitter)s).ColId, (float)newWidth);
                _ColWidthAccumulator.UpdateSize(((ColumnSplitter)s).ColId, newWidth); // update accumulator
                _TotalColWidth = GetTotalColWidth(); // Update the total width of the cache
                
                base.OnPointerReleased(e);

                // You need to refresh after the mouse is released, otherwise these objects will still be in their original positions.
                CalcVisualColRegion();
                UpdateColSplitter();
                UpdateClearHighlightRange();
                //InvalidateArrange();
            };
        }

        // Replace
        _ColSplitters = newColSplitters;
    }
}