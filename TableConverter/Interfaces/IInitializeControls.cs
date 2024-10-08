﻿using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace TableConverter.Interfaces
{
    public interface IInitializeControls
    {
        public Collection<Control> Controls { get; set; }

        public void InitializeControls();
    }
}
