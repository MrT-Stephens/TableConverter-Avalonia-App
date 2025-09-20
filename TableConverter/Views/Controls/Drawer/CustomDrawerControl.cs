using Avalonia.Controls.Primitives;
using TableConverter.Views.Controls.OverlayShared.Interfaces;

namespace TableConverter.Views.Controls.Drawer;

public class CustomDrawerControl : DrawerControlBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_CloseButton is not null)
        {
            _CloseButton.IsVisible = IsCloseButtonVisible ?? true;
        }
    }

    public override void Close()
    {
        if (DataContext is IDialogContext context)
        {
            context.Close();
        }
        else
        {
            OnElementClosing(this, null);
        }
    }

    protected internal override void AnchorAndUpdatePositionInfo()
    {
        // throw new NotImplementedException();
    }
}
