using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace TableConverter.Views
{
    public partial class DialogHostOptionsView : UserControl
    {
        public static readonly StyledProperty<double> MaxDialogWidthProperty = 
            Border.MaxWidthProperty.AddOwner<DialogHostOptionsView>();

        public double MaxDialogWidth
        {
            get => GetValue(MaxDialogWidthProperty);
            set => SetValue(MaxDialogWidthProperty, value);
        }

        public static readonly StyledProperty<double> MaxDialogHeightProperty = 
            Border.MaxHeightProperty.AddOwner<DialogHostOptionsView>();

        public double MaxDialogHeight
        {
            get => GetValue(MaxDialogHeightProperty);
            set => SetValue(MaxDialogHeightProperty, value);
        }

        public static readonly StyledProperty<double> MinDialogWidthProperty = 
            Border.MinWidthProperty.AddOwner<DialogHostOptionsView>();

        public double MinDialogWidth
        {
            get => GetValue(MinDialogWidthProperty);
            set => SetValue(MinDialogWidthProperty, value);
        }

        public static readonly StyledProperty<double> MinDialogHeightProperty = 
            Border.MinHeightProperty.AddOwner<DialogHostOptionsView>();

        public double MinDialogHeight
        {
            get => GetValue(MinDialogHeightProperty);
            set => SetValue(MinDialogHeightProperty, value);
        }

        public static readonly StyledProperty<double> DialogWidthProperty = 
            Border.WidthProperty.AddOwner<DialogHostOptionsView>();

        public double DialogWidth
        {
            get => GetValue(DialogWidthProperty);
            set => SetValue(DialogWidthProperty, value);
        }

        public static readonly StyledProperty<double> DialogHeightProperty = 
            Border.HeightProperty.AddOwner<DialogHostOptionsView>();

        public double DialogHeight
        {
            get => GetValue(DialogHeightProperty);
            set => SetValue(DialogHeightProperty, value);
        }

        public static readonly StyledProperty<string?> TitleProperty = 
            TextBlock.TextProperty.AddOwner<DialogHostOptionsView>();

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DirectProperty<DialogHostOptionsView, IEnumerable> DialogOptionsProperty = 
            AvaloniaProperty.RegisterDirect<DialogHostOptionsView, IEnumerable>(
                nameof(DialogOptions),
                o => o.DialogOptions,
                (o, v) => o.DialogOptions = v);

        private IEnumerable _DialogOptions = new Collection<Control>();

        public IEnumerable DialogOptions
        {
            get => _DialogOptions;
            set => SetAndRaise(DialogOptionsProperty, ref _DialogOptions, value);
        }

        public static readonly StyledProperty<Action> CancelButtonClickProperty = 
            AvaloniaProperty.Register<DialogHostOptionsView, Action>(nameof(CancelButtonClicked));

        public Action CancelButtonClick
        {
            get => GetValue(CancelButtonClickProperty);
            set => SetValue(CancelButtonClickProperty, value);
        }

        public static readonly StyledProperty<Action> OkButtonClickProperty = 
            AvaloniaProperty.Register<DialogHostOptionsView, Action>(nameof(OkButtonClick));
        
        public Action OkButtonClick
        {
            get => GetValue(OkButtonClickProperty);
            set => SetValue(OkButtonClickProperty, value);
        }

        public DialogHostOptionsView()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void CancelButtonClicked(object? sender, RoutedEventArgs e)
        {
            CancelButtonClick?.Invoke();
            DialogOptions = new Collection<Control>();
        }

        private void OkButtonClicked(object? sender, RoutedEventArgs e)
        {
            OkButtonClick?.Invoke();
            DialogOptions = new Collection<Control>();
        }
    }
}
