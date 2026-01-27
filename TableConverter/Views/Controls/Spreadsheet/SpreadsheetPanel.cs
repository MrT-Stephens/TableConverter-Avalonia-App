using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace TableConverter.Views.Controls.Spreadsheet;

/// <summary>
/// Base class for controls that can contain multiple children.
/// </summary>
/// <remarks>
/// Controls can be added to a <see cref="SpreadsheetPanel"/> by adding them to its <see cref="Children"/>
/// collection. All children are layed out to fill the SpreadsheetPanel.
/// </remarks>
public class SpreadsheetPanel : Control, IChildIndexProvider
{
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<SpreadsheetPanel>();

    /// <summary>
    /// Initializes static members of the <see cref="SpreadsheetPanel"/> class.
    /// </summary>
    static SpreadsheetPanel()
    {
        AffectsRender<SpreadsheetPanel>(BackgroundProperty);
    }

    private EventHandler<ChildIndexChangedEventArgs>? _ChildIndexChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadsheetPanel"/> class.
    /// </summary>
    public SpreadsheetPanel()
    {
        Children.CollectionChanged += ChildrenChanged;
    }

    /// <summary>
    /// Gets the children of the <see cref="SpreadsheetPanel"/>.
    /// </summary>
    [Content]
    public Avalonia.Controls.Controls Children { get; } = [];

    /// <summary>
    /// Gets or Sets SpreadsheetPanel background brush.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets whether the <see cref="SpreadsheetPanel"/> hosts the items created by an <see cref="ItemsPresenter"/>.
    /// </summary>
    public bool IsItemsHost { get; internal set; }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_ChildIndexChanged is null)
                Children.PropertyChanged += ChildrenPropertyChanged;
            
            _ChildIndexChanged += value;
        }

        remove
        {
            _ChildIndexChanged -= value;
            
            if (_ChildIndexChanged is null)
                Children.PropertyChanged -= ChildrenPropertyChanged;
        }
    }

    /// <summary>
    /// Renders the visual to a <see cref="DrawingContext"/>.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// 必须去掉 sealed
    public override void Render(DrawingContext context)
    {
        var background = Background;
        if (background != null)
        {
            var renderSize = Bounds.Size;
            context.FillRectangle(background, new Rect(renderSize));
        }

        base.Render(context);
    }

    /// <summary>
    /// Marks a property on a child as affecting the parent SpreadsheetPanel's arrangement.
    /// </summary>
    /// <param name="properties">The properties.</param>
    protected static void AffectsParentArrange<TSpreadsheetPanel>(params AvaloniaProperty[] properties)
        where TSpreadsheetPanel : SpreadsheetPanel
    {
        var invalidateObserver =
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(static e =>
                AffectsParentArrangeInvalidate<TSpreadsheetPanel>(e));
        
        foreach (var property in properties)
        {
            property.Changed.Subscribe(invalidateObserver);
        }
    }

    /// <summary>
    /// Marks a property on a child as affecting the parent SpreadsheetPanel's measurement.
    /// </summary>
    /// <param name="properties">The properties.</param>
    protected static void AffectsParentMeasure<TSpreadsheetPanel>(params AvaloniaProperty[] properties)
        where TSpreadsheetPanel : SpreadsheetPanel
    {
        var invalidateObserver =
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(static e =>
                AffectsParentMeasureInvalidate<TSpreadsheetPanel>(e));
        
        foreach (var property in properties)
        {
            property.Changed.Subscribe(invalidateObserver);
        }
    }

    /// <summary>
    /// Called when the <see cref="Children"/> collection changes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (!IsItemsHost)
                {
                    LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>().ToList());
                }

                VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Move:
                if (!IsItemsHost)
                {
                    LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                }

                VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                if (!IsItemsHost)
                {
                    LogicalChildren.RemoveAll(e.OldItems!.OfType<Control>().ToList());
                }

                VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    if (!IsItemsHost)
                    {
                        LogicalChildren[index] = child;
                    }

                    VisualChildren[index] = child;
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }

        _ChildIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasureOnChildrenChanged();
    }

    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }

    private void ChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
            _ChildIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
    }

    private static void AffectsParentArrangeInvalidate<TSpreadsheetPanel>(AvaloniaPropertyChangedEventArgs e)
        where TSpreadsheetPanel : SpreadsheetPanel
    {
        var control = e.Sender as Control;
        var panelX = control?.GetVisualParent() as TSpreadsheetPanel;
        panelX?.InvalidateArrange();
    }

    private static void AffectsParentMeasureInvalidate<TSpreadsheetPanel>(AvaloniaPropertyChangedEventArgs e)
        where TSpreadsheetPanel : SpreadsheetPanel
    {
        var control = e.Sender as Control;
        var panelX = control?.GetVisualParent() as TSpreadsheetPanel;
        panelX?.InvalidateMeasure();
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is Control control ? Children.IndexOf(control) : -1;
    }

    /// <inheritdoc />
    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count;
        return true;
    }
}
