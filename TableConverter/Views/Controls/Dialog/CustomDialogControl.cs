using Avalonia.Controls.Primitives;
using TableConverter.Extenstions;
using TableConverter.Views.Controls.OverlayShared.Interfaces;

namespace TableConverter.Views.Controls.Dialog;

public class CustomDialogControl : DialogControlBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var closeButtonVisible = IsCloseButtonVisible ?? DataContext is IDialogContext;

        IsHitTestVisibleProperty.SetValue(closeButtonVisible, _CloseButton);

        if (!closeButtonVisible)
        {
            OpacityProperty.SetValue(0, _CloseButton);
        }
    }

    public override void Close()
    {
        if (DataContext is IDialogContext context)
            context.Close();
        else
            OnElementClosing(this, null);
    }
}