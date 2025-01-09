using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public partial class ConvertFilesManager : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ConvertDocumentViewModel> _Files = new();
}