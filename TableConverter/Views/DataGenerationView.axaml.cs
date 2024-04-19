using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.Interfaces;
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
            if (DataContext is DataGenerationViewModel data_generation_view_model)
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

                        DataGenerationDialogHost.CurrentSession?.Close();
                    }
                };

                var resize_func = new EventHandler<SizeChangedEventArgs>((sender, e) => data_generation_types_selector_view.HandleResize());

                SizeChanged += resize_func;

                await DialogHostAvalonia.DialogHost.Show(data_generation_types_selector_view, DataGenerationDialogHost);

                SizeChanged -= resize_func;
            }
        }
    }
}
