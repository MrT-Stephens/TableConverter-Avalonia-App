using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class ConvertDocumentViewModel : ViewModelBase
{
    // General Items
    public IEnumerable<string> ProgressStepValues { get; } = 
    [
        "Input File", "Edit Tabular Data", "Output File"
    ];

    [ObservableProperty]
    private bool _IsBusy = false;

    [ObservableProperty]
    private int _ProgressStepIndex = 0;

    [ObservableProperty]
    private string _Name = string.Empty;

    [ObservableProperty]
    private string _Path = string.Empty;

    [ObservableProperty]
    private bool _IsGenerated = false;

    // Input Items
    [ObservableProperty]
    private ConverterType? _InputConverter = null;

    [ObservableProperty]
    private TextDocument _InputFileText = new();

    // Edit Items
    [ObservableProperty]
    private ObservableCollection<string> _EditHeaders = [];

    [ObservableProperty]
    private ObservableCollection<string[]> _EditRows = [];

    // Output Items
    [ObservableProperty]
    private ConverterType? _OutputConverter = null;

    [ObservableProperty]
    private TextDocument _OutputFileText = new();
}
