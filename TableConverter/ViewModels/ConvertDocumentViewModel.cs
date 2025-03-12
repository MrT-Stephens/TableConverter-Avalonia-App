using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class ConvertDocumentViewModel : ObservableObject
{
    // Edit Items
    [ObservableProperty] private ObservableCollection<string> _EditHeaders = [];
    
    [ObservableProperty] private ObservableCollection<string[]> _EditRows = [];
    
    // Input Items
    [ObservableProperty] private ConverterType? _InputConverter;
    
    [ObservableProperty] private TextDocument _InputFileText = new();
    
    // Output Items
    [ObservableProperty] private ConverterType? _OutputConverter;
    
    [ObservableProperty] private TextDocument _OutputFileText = new();
    
    // General Items
    [ObservableProperty] private string _Id = string.Empty;
    
    [ObservableProperty] private string _Path = string.Empty;
    
    [ObservableProperty] private int _ProgressStepIndex;
    
    [ObservableProperty] private bool _IsBusy;
    
    [ObservableProperty] private bool _IsGenerated;
    
    [ObservableProperty] private string _Name = string.Empty;
    
    // General Items
    public IEnumerable<string> ProgressStepValues { get; } =
    [
        "Input File", "Edit Tabular Data", "Output File"
    ];
}