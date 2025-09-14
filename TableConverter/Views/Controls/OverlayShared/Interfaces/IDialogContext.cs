using System;

namespace TableConverter.Views.Controls.OverlayShared.Interfaces;

public interface IDialogContext
{
    public void Close();

    public event EventHandler<object?>? RequestClose;
}
