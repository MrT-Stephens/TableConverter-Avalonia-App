using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace TableConverter.Views.Controls.Spreadsheet;

[TemplatePart(Name = PART_GridCanvas, Type = typeof(SpreadsheetCanvas))]
[TemplatePart(Name = PART_SheetName, Type = typeof(TextBox))]
[TemplatePart(Name = PART_VerScrollbar, Type = typeof(ScrollBar))]
[TemplatePart(Name = PART_HorScrollbar, Type = typeof(ScrollBar))]
public class Spreadsheet : TemplatedControl
{
    private const string PART_GridCanvas = "PART_GridCanvas";
    private const string PART_SheetName = "PART_SheetName";
    private const string PART_VerScrollbar = "PART_VerScrollbar";
    private const string PART_HorScrollbar = "PART_HorScrollbar";
    
    public static readonly StyledProperty<ISheetData> SheetDataProperty =
        AvaloniaProperty.Register<Spreadsheet, ISheetData>(nameof(SheetData));

    public ISheetData SheetData
    {
        get => GetValue(SheetDataProperty);
        set => SetValue(SheetDataProperty, value);
    }
    
    public static readonly StyledProperty<int> HeadRowIdProperty =
        AvaloniaProperty.Register<Spreadsheet, int>(nameof(HeadRowId), 0);

    public int HeadRowId
    {
        get => GetValue(HeadRowIdProperty);
        set => SetValue(HeadRowIdProperty, value);
    }

    private SpreadsheetGrid? _GridCanvas;
    private TextBox? _SheetNameTextBox;
    private ScrollBar? _VerScrollbar;
    private ScrollBar? _HorScrollbar;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e){
        base.OnApplyTemplate(e);

        _GridCanvas = e.NameScope.Find<SpreadsheetGrid>(PART_GridCanvas);
        _SheetNameTextBox = e.NameScope.Find<TextBox>(PART_SheetName);
        _VerScrollbar = e.NameScope.Find<ScrollBar>(PART_VerScrollbar);
        _HorScrollbar = e.NameScope.Find<ScrollBar>(PART_HorScrollbar);

        if (_GridCanvas == null || _VerScrollbar == null || _HorScrollbar == null || _SheetNameTextBox == null)
        {
            throw new InvalidOperationException("gridCanvas == null || verScrollbar == null || horScrollbar == null");
        }

        _GridCanvas.SheetData = SheetData;
        _GridCanvas.HeadRowId = HeadRowId;
        
        _SheetNameTextBox.Text = SheetData.SheetName;

        _HorScrollbar.Scroll += (_, args) =>
        {
            _GridCanvas.OnHorizontalScrollBarScroll(args.NewValue);
        };

        _VerScrollbar.Scroll += (_, args) => 
        {
            Console.WriteLine($"OnVerticalScrollBarScroll() {args.NewValue}");
            _GridCanvas.OnVerticalScrollBarScroll(args.NewValue);
        };

        Loaded += (_, _) =>
        {
            _GridCanvas.AfterLoad();
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == SheetDataProperty && _GridCanvas is not null)
        {
            _GridCanvas.SheetData = SheetData;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e){
        
        this.Focus();
        Console.WriteLine($"OnPointerCaptureLost() {e}");
    }
}