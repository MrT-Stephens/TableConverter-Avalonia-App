using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TableConverter.Views.Controls.Shapes;

public class PureRectangle : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<PureRectangle>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    static PureRectangle()
    {
        AffectsRender<PureRectangle>(BackgroundProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Background, null, new Rect(Bounds.Size));
    }
}
