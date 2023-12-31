using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using TableConverter.ViewModels;
using System.Linq;
using TableConverter.DataModels;

namespace TableConverter.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        SizeChanged += MainViewSizeChanged;
    }

    /// <summary>
    /// Open file button clicked event handler.
    /// Will open the file picker and allow the user to select a file.
    /// Then it will convert the file to a DataTable and display it in the DataGrid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OpenFileButtonClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            var top_level_window = TopLevel.GetTopLevel(this);

            var files = await top_level_window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>{
                    new($"{view.OutputSelectedConverterType.name} ({view.OutputSelectedConverterType.extension})")
                {
                    Patterns = new[]{$"*{view.InputSelectedConverterType.extension}"},
                }}
            });

            if (files.Count >= 1)
            {
                LockUnlockItems(true);

                view.DataTable = await view.InputSelectedConverterType.converter_handler.ConvertAsync(files[0]);

                LockUnlockItems(false);
            }

            RefreshDataTable();
        }
    }

    private void RefreshDataTable()
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            while (TableDataDataGrid.Columns.Count > 0)
            {
                TableDataDataGrid.Columns.RemoveAt(TableDataDataGrid.Columns.Count - 1);
            }

            TableDataDataGrid.ItemsSource = view.DataTable.DefaultView;

            foreach (System.Data.DataColumn x in view.DataTable.Columns)
            {
                TableDataDataGrid.Columns.Add(new DataGridTextColumn { Header = x.ColumnName, Binding = new Avalonia.Data.Binding($"Row.ItemArray[{x.Ordinal}]") });
            }
        }
    }

    /// <summary>
    /// Save file button clicked event handler.
    /// Will open the file picker and allow the user to select a file location.
    /// Then it will save the converted data to the file at the selected location.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void SaveFileButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable.Rows.Count != 0 && view.DataTable.Columns.Count != 0)
            {
                var top_level_window = TopLevel.GetTopLevel(this);

                var file = await top_level_window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save File",
                    DefaultExtension = view.OutputSelectedConverterType.extension,
                    ShowOverwritePrompt = true,
                    SuggestedFileName = $"{App.Current?.Name}_{DateTime.Now.ToFileTime()}",
                    FileTypeChoices = new List<FilePickerFileType>{
                    new($"{view.OutputSelectedConverterType.name} ({view.OutputSelectedConverterType.extension})")
                {
                    Patterns = new[]{$"*{view.OutputSelectedConverterType.extension}"}
                }}
                });

                await view.OutputSelectedConverterType.converter_handler.SaveFileAsync(file, view.ConvertedData);
            }
        }
    }

    /// <summary>
    /// Input converter type selection changed event handler.
    /// Will update the selected input converter type in the view model.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void InputConverterTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            view.InputSelectedConverterType = (ConverterType)args.AddedItems[0];

            CheckIfConverterTypesEqual();
        }
    }

    /// <summary>
    /// Output converter type selection changed event handler.
    /// Will update the selected output converter type in the view model.
    /// Then it will update the output data options wrap panel with the controls for the selected converter type.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OutputConverterTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            view.OutputSelectedConverterType = (ConverterType)args.AddedItems[0];

            OutputDataOptionsWrapPanel.Children.Clear();
            OutputDataOptionsWrapPanel.Children.AddRange(view.OutputSelectedConverterType.converter_handler.Controls);

            CheckIfConverterTypesEqual();
        }
    }

    /// <summary>
    /// Check if converter types equal event handler.
    /// Will check if the selected input and output converter types are equal.
    /// If they are equal, it will hide the output converter type view.
    /// </summary>
    private void CheckIfConverterTypesEqual()
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.InputSelectedConverterType == view.OutputSelectedConverterType)
            {
                OutputConverterTypeView.IsVisible = false;
            }
            else
            {
                OutputConverterTypeView.IsVisible = true;
            }
        }
    }

    /// <summary>
    /// Input data options button clicked event handler.
    /// Will show or hide the input data options wrap panel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void InputDataOptionsButtonClicked(object sender, RoutedEventArgs args)
    {
        if (InputDataOptionsButton.Content?.ToString() == "Show Options")
        {
            InputDataOptionsButton.Content = "Hide Options";
            InputDataOptionsWrapPanel.IsVisible = true;
        }
        else
        {
            InputDataOptionsButton.Content = "Show Options";
            InputDataOptionsWrapPanel.IsVisible = false;
        }
    }

    /// <summary>
    /// Output data options button clicked event handler.
    /// Will show or hide the output data options wrap panel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OutputDataOptionsButtonClicked(object sender, RoutedEventArgs args)
    {
        if (OutputDataOptionsButton.Content?.ToString() == "Show Options")
        {
            OutputDataOptionsButton.Content = "Hide Options";
            OutputDataOptionsWrapPanel.IsVisible = true;
        }
        else
        {
            OutputDataOptionsButton.Content = "Show Options";
            OutputDataOptionsWrapPanel.IsVisible = false;
        }
    }

    /// <summary>
    /// Convert button clicked event handler.
    /// Will convert the data in the DataGrid to the selected output converter type.
    /// Then it will display the converted data in the TextBox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ConvertButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable.Rows.Count != 0 && view.DataTable.Columns.Count != 0)
            {
                LockUnlockItems(true);

                view.ConvertedData = await view.OutputSelectedConverterType.converter_handler.ConvertAsync(view.DataTable, ConvertTimeProgressBar);

                LockUnlockItems(false);

                ConvertTimeProgressBar.Value = 0;

                ConvertedDataTextBox.CaretIndex = ConvertedDataTextBox.Text.Length;
            }
        }
    }

    /// <summary>
    /// Menu button clicked event handler.
    /// Will open or close the menu split view.
    /// It will also change the menu arrow icon to point in the correct direction.
    /// If the device is a mobile device, it will open the menu split view to the full width of the screen.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void MenuButtonClicked(object sender, RoutedEventArgs args)
    {
        var top_level_window = TopLevel.GetTopLevel(this);

        if (sender is Button && top_level_window != null)
        {
            ResizeMenuSplitView();

            if (MenuSplitView.IsPaneOpen)
            {
                MenuArrowIcon.Data = Geometry.Parse("M450 600L650 800V400z");
                MenuSplitView.IsPaneOpen = false;
            }
            else
            {
                MenuArrowIcon.Data = Geometry.Parse("M700 600L500 400V800z");
                MenuSplitView.IsPaneOpen = true;
            }
        }
    }

    /// <summary>
    /// Menu split view loaded event handler.
    /// Will resize the menu split view to the correct width.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuSplitViewLoaded(object sender, RoutedEventArgs e)
    {
        var top_level_window = TopLevel.GetTopLevel(this);

        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            MenuSplitView.OpenPaneLength = top_level_window.ClientSize.Width;
            MenuArrowIcon.Data = Geometry.Parse("M450 600L650 800V400z");
            MenuSplitView.DisplayMode = SplitViewDisplayMode.Inline;
            MainGrid.Margin = new Avalonia.Thickness(0, 0, 0, 0);
            MenuSplitView.IsPaneOpen = false;
        }
        else
        {
            MenuSplitView.OpenPaneLength = (top_level_window.ClientSize.Width / 3);
            MenuArrowIcon.Data = Geometry.Parse("M700 600L500 400V800z");
            MenuSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
            MainGrid.Margin = new Avalonia.Thickness(30, 0, 30, 0);
            MenuSplitView.IsPaneOpen = true;
        }
    }

    /// <summary>
    /// Resize menu split view function.
    /// Will resize the menu split view to the correct width, depending on the device.
    /// </summary>
    private void ResizeMenuSplitView()
    {
        var top_level_window = TopLevel.GetTopLevel(this);

        if (top_level_window != null)
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                MenuSplitView.OpenPaneLength = top_level_window.ClientSize.Width;
            }
            else
            {
                MenuSplitView.OpenPaneLength = (top_level_window.ClientSize.Width / 3);
            }
        }
    }

    /// <summary>
    /// Locks or unlocks the items which could cause issues if changed while converting.
    /// </summary>
    /// <param name="lock_items"></param>
    private void LockUnlockItems(bool lock_items)
    {
        InputButtonsStackPanel.IsEnabled = !lock_items;
        InputDataOptionsWrapPanel.IsEnabled = !lock_items;
        OutputDataOptionsWrapPanel.IsEnabled = !lock_items;
        OutputButtonsStackPanel.IsEnabled = !lock_items;
        ConvertButton.IsEnabled = !lock_items;
    }

    /// <summary>
    /// Menu split view size changed event handler.
    /// Will resize the menu split view to the correct width.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainViewSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ResizeMenuSplitView();
    }

    /// <summary>
    /// Example button clicked event handler.
    /// Will load the example data for the selected input converter type.
    /// Then it will display the example data in the DataGrid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void ExampleButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            LockUnlockItems(true);

            view.DataTable = await view.InputSelectedConverterType.converter_handler.LoadExampleAsync();

            LockUnlockItems(false);

            RefreshDataTable();
        }
    }

    /// <summary>
    /// Clear button clicked event handler.
    /// Will clear the data in the DataGrid and the TextBox.
    /// It will also reset the progress bar.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ClearButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            view.DataTable = new System.Data.DataTable();

            while (TableDataDataGrid.Columns.Count > 0)
            {
                TableDataDataGrid.Columns.RemoveAt(TableDataDataGrid.Columns.Count - 1);
            }

            TableDataDataGrid.ItemsSource = view.DataTable.DefaultView;

            view.ConvertedData = string.Empty;
            ConvertTimeProgressBar.Value = 0;
        }
    }

    /// <summary>
    /// Copy button clicked event handler.
    /// Will copy the converted data to the clipboard.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void CopyButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var top_level_window = TopLevel.GetTopLevel(this);

            var view = (MainViewModel)DataContext;

            if (view.ConvertedData != string.Empty && top_level_window != null)
            {
                await top_level_window.Clipboard.SetTextAsync(view.ConvertedData);
            }
        }
    }

    private async void CapitalizeButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Capitalize(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void LowercaseButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Lowercase(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void UppercaseButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Uppercase(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void TransposeButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Transpose(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void DeleteSpacesButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.DeleteSpaces(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void DeleteDuplicateRowsButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.DeleteDuplicateRows(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void UndoButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Undo(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private async void RedoButtonClicked(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainViewModel)
        {
            var view = (MainViewModel)DataContext;

            if (view.DataTable != null)
            {
                view.TableConverterService.Redo(view.DataTable);

                RefreshDataTable();
            }
        }
    }

    private void GitHubButtonClicked(object sender, RoutedEventArgs args)
    {
        OpenUrl("https://github.com/MrT-Stephens");
    }

    private void LinkedinButtonClicked(object sender, RoutedEventArgs args)
    {
        OpenUrl("https://www.linkedin.com/in/mrt-stephens");
    }

    private void OpenUrl(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("x-www-browser", url);
            }
            else if (OperatingSystem.IsMacOS())
            {

            }
            else if (OperatingSystem.IsAndroid())
            {

            }
            else if (OperatingSystem.IsIOS())
            {

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private void SearchBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DataContext is MainViewModel && sender is AutoCompleteBox)
        {
            var view = (MainViewModel)DataContext;
            var auto_complete_box = (AutoCompleteBox)sender;

            if (auto_complete_box.SelectedItem != null)
            {
                string[] strings = auto_complete_box.SelectedItem.ToString().Split(" to ");

                view.InputSelectedConverterType = view.InputConverterTypes.Where((converter_type) => converter_type.name == strings[0]).First();

                view.OutputSelectedConverterType = view.OutputConverterTypes.Where((converter_type) => converter_type.name == strings[1]).First();
            }
        }
    }
}
