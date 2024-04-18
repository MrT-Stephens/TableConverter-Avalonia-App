using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TableConverter.DataModels;
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
                        DataGenerationTypes = new()
                        {
                            new DataGenerationType("First Name", "Personal", "First name of a person"),
                            new DataGenerationType("Last Name", "Personal", "Last name of a person"),
                            new DataGenerationType("Full Name", "Personal", "Full name of a person"),
                            new DataGenerationType("Email", "Personal", "Email address of a person"),
                            new DataGenerationType("Phone Number", "Personal", "Phone number of a person"),
                            new DataGenerationType("Address", "Personal", "Address of a person"),
                            new DataGenerationType("City", "Personal", "City of a person"),
                            new DataGenerationType("State", "Personal", "State of a person"),
                            new DataGenerationType("Zip Code", "Personal", "Zip code of a person"),
                            new DataGenerationType("Country", "Personal", "Country of a person"),
                            new DataGenerationType("Date of Birth", "Personal", "Date of birth of a person"),
                            new DataGenerationType("IPv4 Address", "Network", "IPv4 address"),
                            new DataGenerationType("IPv6 Address", "Network", "IPv6 address"),
                            new DataGenerationType("URL", "Network", "URL"),
                            new DataGenerationType("Username", "Network", "Username"),
                            new DataGenerationType("Password", "Network", "Password"),
                            new DataGenerationType("Credit Card Number", "Financial", "Credit card number"),
                            new DataGenerationType("Credit Card Expiry Date", "Financial", "Credit card expiry date"),
                            new DataGenerationType("Credit Card CVV", "Financial", "Credit card CVV"),
                            new DataGenerationType("Credit Card Holder Name", "Financial", "Credit card holder name"),
                            new DataGenerationType("Credit Card Type", "Financial", "Credit card type"),
                            new DataGenerationType("Credit Card Brand", "Financial", "Credit card brand"),
                            new DataGenerationType("Row Number", "System", "Row number"),
                            new DataGenerationType("Random Number", "System", "Random number"),
                            new DataGenerationType("Random String", "System", "Random string"),
                            new DataGenerationType("Random Date", "System", "Random date"),
                            new DataGenerationType("Random Time", "System", "Random time"),
                            new DataGenerationType("Random Date and Time", "System", "Random date and time"),
                            new DataGenerationType("Random Boolean", "System", "Random boolean"),
                            new DataGenerationType("Random GUID", "System", "Random GUID")
                        }
                    },
                    TypesSelectorViewClose = (DataGenerationType? data_generation_type) =>
                    {
                        if (data_generation_type is not null)
                        {
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
