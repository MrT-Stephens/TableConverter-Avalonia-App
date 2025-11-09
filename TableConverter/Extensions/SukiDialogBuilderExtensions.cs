using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace TableConverter.Extensions;

public static class SukiDialogBuilderExtensions
{
    public static SukiDialogBuilder WithForm<TForm>(this SukiDialogBuilder builder, TForm form)
        where TForm : ObservableObject
    {
        builder.WithContent(new PropertyGrid
        {
            Item = form,
            DataTemplates =
            {
                new PropertyGridTemplateSelector
                {
                    UseSukiHost = true,
                }
            },
        });

        return builder;
    }
}