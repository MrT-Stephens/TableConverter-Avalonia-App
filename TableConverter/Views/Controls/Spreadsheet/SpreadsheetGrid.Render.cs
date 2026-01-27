using System.Drawing;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace TableConverter.Views.Controls.Spreadsheet;

public partial class SpreadsheetGrid
{
    public override void Render(DrawingContext dc){
        // The rendering function cannot have any Children.Add(control)
        // Prompt System.InvalidOperationException: Visual was invalidated during the render pass
        // So all objects must be prepared during initialization and events. This is just painting.
        // base.Render(dc) should be drawing the control class
        base.Render(dc);

        // There cannot be any invalid in the rendering function
        //InvalidateVisual();

        // To speed up painting, you need to remove the width of ScrollBar
        var localBounds = new Rect(new Size(
            Bounds.Width,
            Bounds.Height));
        
        var clip = dc.PushClip(localBounds);

        // ----1----
        // 1.1 The entire control draws the background color. One is for debugging, and the other is that the mouse wheel event must have an object to accept it.
        // The wheel event cannot be triggered in an empty place. It is very strange. It is equivalent to this background color as a mask.
        dc.DrawRectangle(Brushes.Transparent, _guideLinePen, localBounds, 1.0d);

        // 1.2 Column header background color
        dc.DrawRectangle(SystemColors.ControlColor,
            _transparentLinePen,
            new Rect(0, 0, localBounds.Width, UiColumnHeaderHeight));

        // 1.2 Outfit background color
        dc.DrawRectangle(SystemColors.ControlColor,
            _transparentLinePen,
            new Rect(0, 0, _uiRowHeaderWidth, localBounds.Height));

        // 2 draws a horizontal line
        double accHeight = 0;

        // 2.1 Column header area
        // Consider the starting position of pixel-level scrolling
        accHeight = UiColumnHeaderHeight;
        dc.DrawLine(_guideLinePen,
            new Point(0, accHeight),
            new Point(localBounds.Width, accHeight));

        // 2.2 Draw table horizontal lines
        if (_VisualRowStartId != -1 && _VisualRowEndId != -1)
        {
            // Calculate the display position of the first line, taking into account pixel-level offset
            accHeight = CalculateRowPositionY(_VisualRowStartId);

            for (var rowId = _VisualRowStartId; rowId <= _VisualRowEndId; rowId++)
            {
                // 2.2.1 Highlight mark headrow background (user setting data)
                if (HeadRowId == rowId)
                {
                    dc.DrawRectangle(SystemColors.PaleBlueColorBrush,
                        _transparentLinePen,
                        new Rect(
                            _uiRowHeaderWidth,
                            accHeight,
                            localBounds.Width - _uiRowHeaderWidth,
                            // Use actual row height, not affected by scrolling
                            SheetData.GetRowHeightInUi((uint)rowId)));
                }

                accHeight += SheetData.GetRowHeightInUi((uint)rowId);
                dc.DrawLine(_guideLinePen,
                    new Point(0, accHeight),
                    new Point(localBounds.Width, accHeight));
            }
        }

        // 3 draw vertical lines
        double accWidth = 0;

        // 3.1 Column header
        accWidth += _uiRowHeaderWidth;
        dc.DrawLine(_guideLinePen,
            new Point(accWidth, 0),
            new Point(accWidth, localBounds.Height));

        // 3.2 Draw table vertical lines
        for (var colId = _VisualColStartId; colId <= _VisualColEndId; colId++)
        {
            accWidth += SheetData.GetColWidthInUi((uint)colId);
            
            dc.DrawLine(_guideLinePen,
                new Point(accWidth, 0),
                new Point(accWidth, localBounds.Height));
        }

        // 4. Text
        // 4.1 Line header text, numerical sequence, consider pixel-level scrolling
        accHeight = CalculateRowPositionY(_VisualRowStartId);
        
        if (_VisualRowStartId != -1)
        {
            for (int rowId = _VisualRowStartId; rowId <= _VisualRowEndId; rowId++)
            {
                var formattedText = CreateFormattedText($"{rowId + 1}");
                // Calculate vertical center and right alignment position
                double rowHeight = SheetData.GetRowHeightInUi((uint)rowId);
                double textHeight = formattedText.Height;
                double centerY = accHeight + (rowHeight - textHeight) / 2;
                double rightX = _uiRowHeaderWidth - formattedText.Width - 5; //5px right margin

                dc.DrawText(formattedText, new Point(rightX, centerY));

                accHeight += rowHeight;
            }
        }

        // 4.2 List header text, alphabetical number
        accWidth = _uiRowHeaderWidth;
        
        if (_VisualColStartId != -1)
        {
            for (int colID = _VisualColStartId; colID <= _VisualColEndId; colID++)
            {
                var formattedText = CreateFormattedText($"{ReferenceHelper.ToColumnLetter(colID)}");
                
                // Calculate center position
                double colWidth = SheetData.GetColWidthInUi((uint)colID);
                double textWidth = formattedText.Width;
                double centerX = accWidth + (colWidth - textWidth) / 2;

                dc.DrawText(formattedText, new Point(centerX, 0));
                accWidth += SheetData.GetColWidthInUi((uint)colID);
            }
        }

        // 4.3 Fill content
        // Use a calculation function to get the correct starting position, taking into account pixel-level scrolling
        accHeight = CalculateRowPositionY(_VisualRowStartId);

        // Define the cell content area (excluding row headers and column headers)
        var contentAreaRect = new Rect(
            _uiRowHeaderWidth,
            UiColumnHeaderHeight,
            localBounds.Width - _uiRowHeaderWidth,
            localBounds.Height - UiColumnHeaderHeight);

        //Create a cropping area for the cell content to prevent the content from overflowing to the row and column headers
        using (var contentClip = dc.PushClip(contentAreaRect))
        {
            if (_VisualRowStartId != -1 && _VisualColStartId != -1 && _VisualRowEndId != -1 && _VisualColEndId != -1)
            {
                for (int rowId = _VisualRowStartId; rowId <= _VisualRowEndId; rowId++)
                {
                    var curRowHeight = SheetData.GetRowHeightInUi((uint)rowId);

                    // If the row position is smaller than the column header height, skip the rendering of this row
                    if (accHeight + curRowHeight <= UiColumnHeaderHeight)
                    {
                        continue;
                    }

                    accWidth = _uiRowHeaderWidth;
                    
                    for (int colId = _VisualColStartId; colId <= _VisualColEndId; colId++)
                    {
                        var curColWidth = SheetData.GetColWidthInUi((uint)colId);

                        // Calculate the visible area of the cell
                        var cellRect = new Rect(
                            accWidth,
                            accHeight,
                            curColWidth,
                            curRowHeight);

                        var cellValue = SheetData.GetCellText(rowId, colId);
                        
                        if (string.IsNullOrEmpty(cellValue))
                        {
                            //Performance optimization: Do not render empty content
                        }
                        else if (rowId == _ClickCellRowId && colId == _ClickCellColId && _editorTextBox.IsVisible)
                        {
                            // Enter editing mode and do not render, otherwise the text will overlap
                        }
                        else
                        {
                            // Use clipping to ensure only the visible part of the cell is rendered
                            //The cellRect here is relative to the entire canvas, we need to ensure that it does not render above the column header area
                            using var cellClip = dc.PushClip(cellRect);
                            
                            var textLayout = new TextLayout(
                                cellValue
                                , _typeface
                                , 12
                                , Brushes.Black
                                , TextAlignment.Left
                                , TextWrapping.Wrap,
                                maxWidth: curColWidth,
                                maxHeight: curRowHeight);

                            // Draw text within the cell
                            textLayout.Draw(dc, new Point(
                                accWidth,
                                accHeight));
                        }

                        accWidth += curColWidth;
                    }

                    // Console.WriteLine($"rowID={rowID} height={SheetData.GetRowHeightInExcel((uint)rowID)}");
                    accHeight += curRowHeight; // Accumulate row height
                }
            }
        }

        // The moving horizontal line of the drawing row has nothing to do with the table header and column header.
        if (_IsRowSplitterMoving)
        {
            dc.DrawLine(_splitterMovingLinePen,
                new Point(0, _CursorPoint.Y),
                new Point(localBounds.Width, _CursorPoint.Y));
        }

        if (_IsColSplitterMoving)
        {
            dc.DrawLine(_splitterMovingLinePen,
                new Point(_CursorPoint.X, 0),
                new Point(_CursorPoint.X, localBounds.Height));
        }

        // end drawing the world 

        // this is prime time to draw gui stuff 

        //context.DrawLine(_pen, _cursorPoint + new Vector(-200, 0), _cursorPoint + new Vector(20, 0));

        // This is equivalent to the drag effect of drawing a wide line
        // dc.DrawLine(_pen, _cursorPoint + new Vector(0, -200), _cursorPoint + new Vector(0, 200));

        clip.Dispose();

        // oh and draw again when you can, no rush, right?
        // Either continuous painting, which consumes resources; or InvalidateXXX() after each operation
        // Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    private static FormattedText CreateFormattedText(string textToFormat, double size = 12){
        return new FormattedText(textToFormat,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            size,
            Brushes.Black);
    }
}