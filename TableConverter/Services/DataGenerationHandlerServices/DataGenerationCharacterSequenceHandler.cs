using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationCharacterSequenceHandler : DataGenerationTypeHandlerAbstract
    {
        private string CharacterSequence { get; set; } = string.Empty;

        public override void InitializeOptionsControls()
        {
            var character_sequence_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            var sequence_text_box = new TextBox()
            {
                Watermark = "Example: ^^-@@-##",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = 200,
            };

            sequence_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    CharacterSequence = text_box.Text ?? "";
                }
            };

            var flyout_button = new Button()
            {
                Content = "Help",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            flyout_button.Flyout = new Flyout()
            {
                Content = new TextBlock()
                {
                    Text = "Use the following characters to generate a sequence:\n" +
                           "  - # - Random number.\n" +
                           "  - @ - Random uppercase letter.\n" +
                           "  - ^ - Random lowercase letter.\n" +
                           "  - ? - Random special character.\n" +
                           "  - * - Random number or letter.\n" +
                           "  - $ - Random number or uppercase letter.\n" +
                           "  - % - Random number or lowercase letter.\n" +
                           "  - Any other character will be displayed as is.\n" +
                           "For example:\n" +
                           "  - ##-@@-^^ - Gives you 24-UF-lk\n" +
                           "  - #@#@#@#@ - Gives you 2K6L5J9P\n",
                    FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                },
                Placement = PlacementMode.Bottom,
                ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway
            };

            character_sequence_stack_panel.Children.Add(sequence_text_box);
            character_sequence_stack_panel.Children.Add(flyout_button);

            OptionsControls.Add(character_sequence_stack_panel);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = CheckBlank(GenerateSequence(CharacterSequence), blanks_percentage);
                }

                return data;
            });
        }

        private string GenerateSequence(string sequence)
        {
            string result = "";

            Random random = new Random();

            foreach (char c in sequence)
            {
                switch (c)
                {
                    case '#':
                        result += random.Next(0, 10).ToString();
                        break;
                    case '@':
                        result += (char)random.Next(65, 91);
                        break;
                    case '^':
                        result += (char)random.Next(97, 123);
                        break;
                    case '?':
                        result += (char)random.Next(33, 48);
                        break;
                    case '*':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(65, 123);
                        }
                        break;
                    case '$':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(65, 91);
                        }
                        break;
                    case '%':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(97, 123);
                        }
                        break;
                    default:
                        result += c;
                        break;
                }
            }

            return result;
        }
    }
}
