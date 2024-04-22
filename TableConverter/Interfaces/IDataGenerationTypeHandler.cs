using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public interface IDataGenerationTypeHandler
    {
        public Random Random { get; set; }

        public Collection<Control> OptionsControls { get; set; }

        public void InitializeOptionsControls();

        public Task<string[]> GenerateData(long rows, int blanks_percentage);
    }
}
