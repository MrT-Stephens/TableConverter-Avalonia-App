using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaSheetControl;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace TableConverter.Views.Controls.Spreadsheet;

public partial class SpreadsheetGrid : SpreadsheetCanvas
{
    private readonly XLogger _log = new();
    private ISheetData _SheetData;
    public int HeadRowId = -1;
    public HashSet<int> IgnoredColumnIDs = [];

    private Point _CursorPoint;
    private bool _IsPointerCaptured;

    private double _TotalRowHeight;
    private double _TotalColWidth;


    private readonly IPen _transparentLinePen;
    private readonly IPen _guideLinePen;
    private readonly IPen _splitterMovingLinePen;

    private static FontFamily Default { get; } = new("Inter, -apple-system,BlinkMacSystemFont,PingFang SC, Microsoft YaHei, Segoe UI, Hiragino Sans GB, Helvetica Neue,Helvetica,Arial,sans-serif");

    private readonly Typeface _typeface = new(Default);

    private readonly TextBox _editorTextBox = new()
    {
        Classes = { "ClassInputEditor" }
    };

    private readonly Border _leftTopHeaderTriangle = new()
    {
        Child = new Polygon()
        {
            Points = new Points
            {
                new(UiColumnHeaderHeight - 2, 0),
                new(0, UiColumnHeaderHeight - 2),
                new(UiColumnHeaderHeight - 2, UiColumnHeaderHeight - 2),
            },
            Fill = SystemColors.ControlLightColor,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
        Background = Brushes.Transparent,
    };

    private readonly Border _singleCellSelectionBorder = new()
    {
        BorderThickness = new Thickness(0.5),
        BorderBrush = new SolidColorBrush(new Color(0XFF, 0x21, 0x73, 0x46))
    };

    private readonly Border _singleRowMoveBorder = new()
    {
        BorderThickness = new Thickness(0.5),
        Background = new SolidColorBrush(new Color(0x40, 0x00, 0x96, 0xFF)), // 更深的蓝色背景
        BorderBrush = new SolidColorBrush(new Color(0xFF, 0x00, 0x78, 0xD7)) // 蓝色边框
    };

    /*
    20240921 It is not feasible to use MenuFlyout to create a right-click menu, because MenuFlyout appears when you right-click once.
    Clicking it again will not appear. It will only appear when clicked again. This does not fit the context menu scenario because the control will be emitted.
    pointerRelease event, this can be confirmed through devtool

    The suitable scenario for menuflyout is similar to selecting a certain text in word, using
    _menuFlyout.ShowAt(this, true); Actively displays the toolbar menu with a very good experience
    Floating objects are very special objects. Maybe you find out after debugging that they are not as good as inputtextbox.
    Directly add child to display directly
     */
    private readonly ContextMenu _contextMenu = new()
    {
        Classes = { "ClassContextMenu" },
        Items =
        {
        }
    };


    //Items ={
    // new MenuItem(){
    // Header = "This line is the title",
    // },
    // new MenuItem(){
    // Header = "Ignore this column"
    // },
    // new MenuItem(){
    // Header = "Undo Ignore"
    // }
    // }

    /*
    The most important data: What is the relative value of the current viewport in the entire view?
    Vector Offset in native ScrollViewer
    ScrollX/ScrollY in ReoGrid.IViewport

    !!!
    Note: This X Y coordinate value is relative to the view. There are other things that may exist outside the view.
    Head and other components need to be accurately removed
    !!!
    */
    private double ViewportOffsetX { get; set; }
    private double ViewportOffsetY { get; set; }

    public SpreadsheetGrid()
    {
        ContextMenu = _contextMenu;
        Children.Add(_leftTopHeaderTriangle);
        Children.Add(_editorTextBox);
        Children.Add(_singleCellSelectionBorder);
        Children.Add(_singleRowMoveBorder);
        SetLeft(_leftTopHeaderTriangle, _uiRowHeaderWidth - UiColumnHeaderHeight);

        // public Pen(
        //     uint color,
        //     double thickness = 1.0,
        //     IDashStyle? dashStyle = null,
        //     PenLineCap lineCap = PenLineCap.Flat,
        //     PenLineJoin lineJoin = PenLineJoin.Miter,
        //     double miterLimit = 10.0)

        _transparentLinePen = new Pen(new SolidColorBrush(Colors.Transparent),
            0,
            lineCap: PenLineCap.Round);
        _guideLinePen = new Pen(new SolidColorBrush(Colors.LightGray),
            0.5,
            lineCap: PenLineCap.Round);
        _splitterMovingLinePen = new Pen(new SolidColorBrush(Colors.Black),
            lineCap: PenLineCap.Round,
            dashStyle: DashStyle.Dot);
    }

    // Accumulator
    private CellSizeAccumulator _RowHeightAccumulator;
    private CellSizeAccumulator _ColWidthAccumulator;

    public ISheetData SheetData
    {
        get => _SheetData;
        set
        {
            _SheetData = value;
            // Make sure to initialize the accumulator after QSheetData is set
            _RowHeightAccumulator = new CellSizeAccumulator(i => value.GetRowHeightInUi((uint)i));
            _RowHeightAccumulator.Initialize(value.GetValidRowCount());
            _log.Debug($"RowSizeAccumulator initialized with {value.GetValidRowCount()} rows");

            // initializeColumnWidthAccumulator
            _ColWidthAccumulator = new CellSizeAccumulator(i => value.GetColWidthInUi((uint)i));
            _ColWidthAccumulator.Initialize(value.GetValidColCount());
            _log.Debug($"ColSizeAccumulator initialized with {value.GetValidColCount()} cols");
        }
    }

    // The bound in the constructor is empty and the visible area cannot be calculated, so it needs to be called externally.
    public void AfterLoad()
    {
        _log.Debug("enter method.");

        CalcVisualRowRegion();
        CalcVisualColRegion();
        UpdateRowSplitter();
        UpdateColSplitter();
        UpdateClearHighlightRange();
        InvalidateArrange();
    }

    // There is currently no need to drag and drop in the main interface.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            UpdateClearHighlightRange();

            CalcClickCellRowColId(e.GetPosition(this));

            if (e.ClickCount == 2)
            {
                // Enter edit mode
                UpdateEditorTextBox();
                InvalidateVisual();
            }
            else if (e.ClickCount == 1)
            {
                UpdateHighlightRange();
                InvalidateVisual();
            }
        }
        else
        {
            //_menuFlyout.ShowAt(this,true);

            // 右键菜单，需要特别设置
            //base.OnPointerPressed(e);
        }
    }


    // Must have, the current coordinates are updated to realize the freedom of drawing and dragging
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        Point previousPoint = _CursorPoint;

        _CursorPoint = e.GetPosition(this);

        // Check if the mouse is in the scroll bar area
        bool isInScrollbarArea = _CursorPoint.X > this.Bounds.Width - 26 ||
                                 _CursorPoint.Y > this.Bounds.Height - 26;

        if (isInScrollbarArea)
        {
            _singleRowMoveBorder.IsVisible = false;
        }
        else
        {
            var cursorResult = CalcClickCellRowColId1(_CursorPoint);
            UpdatePointerMove(cursorResult);
        }

        //The coordinates are based on the main window. Even if there are margins and the like outside, the coordinates are stable.
        //Console.WriteLine($"Captured={_isPointerCaptured} _cursorPoint={_cursorPoint} ");
        //Console.WriteLine($"DesiredSize={this.DesiredSize} bound={this.Bounds} {this.IsHitTestVisible} ");

        if (_IsPointerCaptured)
        {
            //Console.WriteLine($"_isPointerCaptured={_isPointerCaptured}");
        }

        //Text box display
        //SetTop(textbox, _cursorPoint.Y);
        //SetLeft(textbox, _cursorPoint.X);
        InvalidateArrange(); // The location information has been changed, InvalidateVisual is invalid
    }
    
    //
    // protected override void OnPointerReleased(PointerReleasedEventArgs e){
    //     e.Pointer.Capture(null);
    //     isPointerCaptured = false;
    //     Console.WriteLine($"OnPointerReleased {e.GetPosition(this)}");
    //     base.OnPointerReleased(e);
    // }

    protected override void OnSizeChanged(SizeChangedEventArgs args)
    {
        _log.Debug("enter method.");

        base.OnSizeChanged(args);

        //At this point, the visible area is calculated
        CalcVisualRowRegion();
        CalcVisualColRegion();
        UpdateRowSplitter();
        UpdateColSplitter();
        InvalidateVisual();
    }

    private int _ClickCellRowId = -1;
    private int _ClickCellColId = -1;
    private double _SingleCellSelectionBorderTop = 0;
    private double _SingleCellSelectionBorderLeft = 0;

    private void CalcClickCellRowColId(Point clickPoint)
    {
        if (SheetData.GetValidRowCount() == 0 || SheetData.GetValidColCount() == 0)
        {
            _VisualColStartId = -1;
            _VisualColEndId = -1;
            return;
        }

        if (_VisualColStartId == -1 || _VisualColEndId == -1)
        {
            return;
        }

        _ClickCellRowId = -1;
        _ClickCellColId = -1;

        double acc = UiColumnHeaderHeight;
        for (var i = _VisualRowStartId; i <= _VisualRowEndId; i++)
        {
            if (acc <= clickPoint.Y && clickPoint.Y < acc + SheetData.GetRowHeightInUi((uint)i))
            {
                _ClickCellRowId = i;
                _SingleCellSelectionBorderTop = acc;
                break;
            }

            acc += SheetData.GetRowHeightInUi((uint)i);
        }

        acc = _uiRowHeaderWidth;
        for (var i = _VisualColStartId; i <= _VisualColEndId; i++)
        {
            if (acc <= clickPoint.X && clickPoint.X < acc + SheetData.GetColWidthInUi((uint)i))
            {
                _ClickCellColId = i;
                _SingleCellSelectionBorderLeft = acc;
                break;
            }

            acc += SheetData.GetColWidthInUi((uint)i);
        }
    }

    // This code is repeated and the logic is different.
    private (int rowID, int colID, double top, double left) CalcClickCellRowColId1(Point clickPoint)
    {
        int rowId = -1;
        int colId = -1;
        double top = 0;
        double left = 0;

        if (SheetData.GetValidRowCount() == 0 || SheetData.GetValidColCount() == 0)
        {
            return (rowId, colId, top, left);
        }

        if (_VisualColStartId == -1 || _VisualColEndId == -1)
        {
            return (rowId, colId, top, left);
        }

        double acc = UiColumnHeaderHeight;
        for (var i = _VisualRowStartId; i <= _VisualRowEndId; i++)
        {
            if (acc <= clickPoint.Y && clickPoint.Y < acc + SheetData.GetRowHeightInUi((uint)i))
            {
                rowId = i;
                top = acc;
                break;
            }

            acc += SheetData.GetRowHeightInUi((uint)i);
        }


        acc = _uiRowHeaderWidth;
        for (var i = _VisualColStartId; i <= _VisualColEndId; i++)
        {
            if (acc <= clickPoint.X && clickPoint.X < acc + SheetData.GetColWidthInUi((uint)i))
            {
                colId = i;
                left = acc;
                break;
            }

            acc += SheetData.GetColWidthInUi((uint)i);
        }

        return (rowId, colId, top, left);
    }

    private void UpdatePointerMove((int rowID, int colID, double top, double left) re)
    {
        if (re.rowID == -1 || re.colID == -1)
        {
            _singleRowMoveBorder.IsVisible = false;
        }
        else
        {
            _singleRowMoveBorder.IsVisible = true;
            _singleRowMoveBorder.Width = this.Bounds.Width;
            _singleRowMoveBorder.Height = SheetData.GetRowHeightInUi((uint)re.rowID);

            // The calculation amount of CalculateRowPositionY here is acceptable, and an accumulator is used, otherwise the appointment will be slower later.
            SpreadsheetCanvas.SetTop(_singleRowMoveBorder, CalculateRowPositionY(re.rowID));
            SpreadsheetCanvas.SetLeft(_singleRowMoveBorder, 0);
        }
    }


    // Calculate the total row height (view layer responsibility)
    private double GetTotalRowHeight() =>
        _RowHeightAccumulator.GetAccumulated(
            SheetData.GetValidRowCount());

    // Calculate the total column width (view layer responsibility)
    private double GetTotalColWidth() =>
        _ColWidthAccumulator.GetAccumulated(
            SheetData.GetValidColCount());
}