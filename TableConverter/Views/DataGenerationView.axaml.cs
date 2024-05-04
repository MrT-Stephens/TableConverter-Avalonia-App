using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.ViewModels;

namespace TableConverter.Views
{
    public partial class DataGenerationView : UserControl
    {
        public DataGenerationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            InitializeComponent(true);
        }

        private async void ChooseFieldTypeButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel data_generation_view_model && Parent is DialogHost dialog_host)
            {
                DataGenerationTypesSelectorView data_generation_types_selector_view = new DataGenerationTypesSelectorView()
                {
                    DataContext = new DataGenerationTypesSelectorViewModel()
                    {
                        DataGenerationTypes = new(data_generation_view_model.DataGenerationTypes.Keys.ToArray()),
                    },
                    TypesSelectorViewClose = (DataGenerationType? data_generation_type) =>
                    {
                        if (DataGenerationFieldsDataGrid.SelectedItem is DataGenerationField field)
                        {
                            if (data_generation_type is not null)
                            {
                                field.TypeHandler = Activator.CreateInstance(data_generation_view_model.DataGenerationTypes[data_generation_type]) as IDataGenerationTypeHandler;
                                field.Type = data_generation_type.Name;
                            }
                            else
                            {
                                field.TypeHandler = null;
                                field.Type = "Choose a Type";
                            }
                        }

                        dialog_host.CurrentSession?.Close();
                    }
                };

                var resize_func = new EventHandler<SizeChangedEventArgs>((sender, e) => data_generation_types_selector_view.HandleResize());

                SizeChanged += resize_func;

                await DialogHost.Show(data_generation_types_selector_view, dialog_host);

                SizeChanged -= resize_func;
            }
        }

        private void MainContentBorderLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Border ctrl)
            {
                if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
                {
                    ctrl.Padding = new Thickness(10, ctrl.Padding.Top, 10, ctrl.Padding.Bottom);
                }
            }
        }

        private async void GenerateDataButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel data_generation_view_model)
            {
                TableData data = await DataGenerationTypesService.GetDataGenerationDataAsync(data_generation_view_model.DataGenerationFields.ToArray(), data_generation_view_model.NumberOfRows);

                data_generation_view_model.ActualInputTextBoxText = "Data generated successfully. The generated data can not be viewed here 😭";

                data_generation_view_model.EditColumnValues = new ObservableCollection<string>(data.headers);
                data_generation_view_model.EditRowValues = new ObservableCollection<string[]>(data.rows);

                data_generation_view_model.CurrentView = new TableConverterView()
                {
                    DataContext = data_generation_view_model
                };
            }
        }

        private void GoBackButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel data_generation_view_model)
            {
                data_generation_view_model.CurrentView = new TableConverterView()
                {
                    DataContext = data_generation_view_model
                };
            }
        }
    }
}
