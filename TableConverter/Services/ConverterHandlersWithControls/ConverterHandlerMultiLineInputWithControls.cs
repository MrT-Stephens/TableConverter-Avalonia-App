using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerMultiLineInputWithControls : ConverterHandlerMultiLineInput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var row_separator_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var row_separator_label = new TextBlock()
            {
                Text = "Row Separator:",
            };

            var row_separator_text_box = new TextBox()
            {
                Text = Options!.RowSeparator,
            };

            row_separator_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    Options!.RowSeparator = text_box.Text ?? "";
                }
            };

            row_separator_stack_panel.Children.Add(row_separator_label);
            row_separator_stack_panel.Children.Add(row_separator_text_box);

            Controls?.Add(row_separator_stack_panel);
        }
    }
}
