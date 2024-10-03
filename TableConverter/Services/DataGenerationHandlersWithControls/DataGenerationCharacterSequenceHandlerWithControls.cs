using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using SukiUI.Controls;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationCharacterSequenceHandlerWithControls : DataGenerationCharacterSequenceHandler, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();

            var sequence_text_box = new TextBox()
            {
                Watermark = "Example: ^^-@@-##",
                MinWidth = 200,
            };

            sequence_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    Options!.CharacterSequence = text_box.Text ?? "";
                }
            };

            var flyout_button = new Button()
            {
                Content = "Help",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            flyout_button.Classes.Add("Flat");

            flyout_button.Flyout = new Flyout()
            {
                PlacementAnchor = PopupAnchor.BottomRight,
                ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
                Content = new GlassCard()
                {
                    IsOpaque = true,
                    Content = new TextBlock()
                    {
                        Text = "Use the following characters to generate a sequence:\n\n" +
                                "\t• # - Random number.\n" +
                                "\t• @ - Random uppercase letter.\n" +
                                "\t• ^ - Random lowercase letter.\n" +
                                "\t• ? - Random special character.\n" +
                                "\t• * - Random number or letter.\n" +
                                "\t• $ - Random number or uppercase letter.\n" +
                                "\t• % - Random number or lowercase letter.\n" +
                                "\t• Any other character will be displayed as is.\n\n" +
                                "For example:\n\n" +
                                "\t• ##-@@-^^ - Gives you 24-UF-lk\n" +
                                "\t• #@#@#@#@ - Gives you 2K6L5J9P",
                    }
                }
            };

            Controls.Add(sequence_text_box);
            Controls.Add(flyout_button);
        }
    }
}
