using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    internal abstract class DataGenerationTypeHandlerAbstract : IDataGenerationTypeHandler
    {
        public Collection<Control> OptionsControls { get; set; }

        protected DataGenerationTypeHandlerAbstract()
        {
            OptionsControls = new Collection<Control>();

            InitializeOptionsControls();
        }

        public virtual void InitializeOptionsControls()
        {
            OptionsControls.Add(new TextBlock()
                {
                    Text = "No options available.",
                    Margin = new Thickness(20, 0, 20, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                });
        }

        public abstract Task<string[]> GenerateData(long rows, int blanks_percentage);
    }
}
