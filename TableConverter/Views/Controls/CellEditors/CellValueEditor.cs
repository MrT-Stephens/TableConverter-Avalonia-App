using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Models;

namespace TableConverter.Views.Controls.CellEditors;

/// <summary>
/// Edits a cell with the control its column's type calls for.
/// </summary>
/// <remarks>
/// <para>
/// A column's type says what its cells hold, so it also says how a value is best entered: a day is
/// picked from a calendar rather than typed, a number is stepped with a spinner, and a yes or no is a
/// tick in a box. Entering a value this way cannot produce one that does not read as the type, which
/// is what keeps a typed column worth having.
/// </para>
/// <para>
/// A cell always stores its value as text, so the editor reads that text into the shape its control
/// wants and writes the control's value back as text in the one form the store's own reading accepts.
/// Text that does not read as the type is shown as nothing rather than being rewritten, so opening a
/// cell that holds something wrong never loses what is stored: the cell itself goes on showing it,
/// marked as not reading as the type.
/// </para>
/// </remarks>
public sealed class CellValueEditor : UserControl
{
    /// <summary>
    /// The stored text the editor stands for.
    /// </summary>
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<CellValueEditor, string?>(nameof(Value));

    /// <summary>
    /// The form a day is written in, which is the one the store reads back as a day.
    /// </summary>
    private const string DayFormat = "yyyy-MM-dd";

    /// <summary>
    /// The form a day and a time are written in, which is the one the store reads back as a moment.
    /// </summary>
    private const string MomentFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly ColumnDataType _dataType;

    private NumericUpDown? _number;
    private CheckBox? _flag;
    private CalendarDatePicker? _day;
    private TimePicker? _time;
    private TextBox? _text;

    /// <summary>
    /// Set while the editor is being shown the stored value, so that showing a value is never mistaken
    /// for the user having entered one.
    /// </summary>
    private bool _showing;

    /// <summary>
    /// Every subscription the editor makes to its own controls, so that they are all released together
    /// when the editor leaves the tree.
    /// </summary>
    private readonly EventRegistrar _events = new();

    public CellValueEditor(ColumnDataType dataType)
    {
        _dataType = dataType;

        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Center;

        Content = Build();
    }

    /// <summary>
    /// The stored text being edited.
    /// </summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Takes up the editor's own controls on the way into the tree.
    /// </summary>
    /// <remarks>
    /// Listening is taken up here rather than once in the constructor because the editor leaves the tree
    /// and comes back to it: the grid builds one as a cell is entered and throws it away as the cell is
    /// left, and a row scrolled out and back brings the same editor in again. Each of the editor's
    /// controls outlives nothing on its own, but a handler left behind on a control the editor has
    /// finished with would keep the editor itself alive, so leaving the tree gives every handler back
    /// (see <see cref="OnDetachedFromVisualTree" />) and coming back to it takes them up once more.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Listen();
    }

    /// <summary>
    /// Gives back every handler the editor holds when it leaves the tree.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _events.ClearAll();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            Show(change.GetNewValue<string?>());
        }
    }

    /// <summary>
    /// Subscribes the editor to the controls it was built with, tracking every subscription so that they
    /// can all be released together.
    /// </summary>
    private void Listen()
    {
        // Cleared first so that coming back into the tree without having left it cannot put a second
        // handler where one already stands.
        _events.ClearAll();

        if (_number is { } number)
        {
            _events.RegisterEvent<EventHandler<NumericUpDownValueChangedEventArgs>>(
                action => number.ValueChanged += action,
                action => number.ValueChanged -= action,
                null, (_, _) => PublishNumber());
        }

        if (_flag is { } flag)
        {
            _events.RegisterEvent<EventHandler<RoutedEventArgs>>(
                action => flag.IsCheckedChanged += action,
                action => flag.IsCheckedChanged -= action,
                null, (_, _) => PublishFlag());
        }

        if (_day is { } day)
        {
            _events.RegisterEvent<EventHandler<SelectionChangedEventArgs>>(
                action => day.SelectedDateChanged += action,
                action => day.SelectedDateChanged -= action,
                null, (_, _) => PublishDay());
        }

        if (_time is { } time)
        {
            _events.RegisterEvent<EventHandler<TimePickerSelectedValueChangedEventArgs>>(
                action => time.SelectedTimeChanged += action,
                action => time.SelectedTimeChanged -= action,
                null, (_, _) => PublishMoment());
        }

        if (_text is { } text)
        {
            _events.RegisterEvent<EventHandler<TextChangedEventArgs>>(
                action => text.TextChanged += action,
                action => text.TextChanged -= action,
                null, (_, _) => PublishText());
        }
    }

    /// <summary>
    /// Builds the control the column's type calls for.
    /// </summary>
    private Control Build()
    {
        return _dataType switch
        {
            ColumnDataType.Integer or ColumnDataType.Decimal => BuildNumber(),
            ColumnDataType.Boolean => BuildFlag(),
            ColumnDataType.Date => BuildDay(),
            ColumnDataType.DateTime => BuildMoment(),
            _ => BuildText(),
        };
    }

    /// <summary>
    /// A number is stepped with a spinner and read off it, so what is entered is a number and not text
    /// that has to be read as one. A whole number steps by one and a fraction by a thousandth.
    /// </summary>
    private Control BuildNumber()
    {
        _number = new NumericUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextAlignment = TextAlignment.Right,
            ParsingNumberStyle = NumberStyles.Number,
            Increment = _dataType is ColumnDataType.Integer ? 1m : 0.001m,
        };

        return _number;
    }

    /// <summary>
    /// A yes or no is a tick in a box, so the value is entered by choosing it rather than by writing
    /// something that has to be read as one.
    /// </summary>
    private Control BuildFlag()
    {
        _flag = new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            IsThreeState = false,
        };

        return _flag;
    }

    /// <summary>
    /// A day is picked from a calendar, which shows the month it falls in and cannot name a day that
    /// does not exist. The day is shown in the one form that cannot be misread.
    /// </summary>
    private Control BuildDay()
    {
        _day = new CalendarDatePicker
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SelectedDateFormat = CalendarDatePickerFormat.Custom,
            CustomDateFormatString = DayFormat,
        };

        return _day;
    }

    /// <summary>
    /// A moment is a day and a time, and a calendar names only a day, so the day is picked from the
    /// calendar and the time from the spinner beside it.
    /// </summary>
    private Control BuildMoment()
    {
        _day = new CalendarDatePicker
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SelectedDateFormat = CalendarDatePickerFormat.Custom,
            CustomDateFormatString = DayFormat,
        };

        _time = new TimePicker
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            ClockIdentifier = "24HourClock",
            UseSeconds = true,
        };

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 6,
        };

        row.Children.Add(_day);
        row.Children.Add(_time);
        Grid.SetColumn(_time, 1);

        return row;
    }

    /// <summary>
    /// A text column takes whatever is typed into it, which is what its type is for.
    /// </summary>
    private Control BuildText()
    {
        _text = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
        };

        return _text;
    }

    /// <summary>
    /// Shows <paramref name="value" /> in the editor, reading the stored text the way the column's
    /// type reads it.
    /// </summary>
    private void Show(string? value)
    {
        _showing = true;

        try
        {
            _dataType.TryReadValue(value, out var typed);

            if (_number is not null)
            {
                _number.Value = typed switch
                {
                    long integer => integer,
                    decimal number => number,
                    _ => null,
                };
            }

            if (_flag is not null)
            {
                _flag.IsChecked = typed is bool flag ? flag : null;
            }

            if (_day is not null)
            {
                _day.SelectedDate = typed switch
                {
                    DateOnly day => day.ToDateTime(TimeOnly.MinValue),
                    DateTime moment => moment.Date,
                    _ => null,
                };
            }

            if (_time is not null)
            {
                _time.SelectedTime = typed is DateTime moment ? moment.TimeOfDay : null;
            }

            if (_text is not null)
            {
                _text.Text = value;
            }
        }
        finally
        {
            _showing = false;
        }
    }

    /// <summary>
    /// Hands the spinner's number back as the text the store keeps.
    /// </summary>
    private void PublishNumber()
    {
        if (_showing)
        {
            return;
        }

        Value = _number?.Value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Hands the box's tick back as the text the store keeps.
    /// </summary>
    private void PublishFlag()
    {
        if (_showing)
        {
            return;
        }

        Value = _flag?.IsChecked switch
        {
            true => bool.TrueString,
            false => bool.FalseString,
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Hands the calendar's day back as the text the store keeps.
    /// </summary>
    /// <remarks>
    /// A calendar used for a moment names only half of what the cell holds, so its day is handed over
    /// together with the time beside it rather than on its own.
    /// </remarks>
    private void PublishDay()
    {
        if (_showing)
        {
            return;
        }

        if (_time is not null)
        {
            PublishMoment();
            return;
        }

        Value = _day?.SelectedDate?.ToString(DayFormat, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Hands the calendar's day and the spinner's time back together as the text the store keeps.
    /// </summary>
    /// <remarks>
    /// A time on its own names no moment, so a day that has been cleared clears the cell rather than
    /// keeping a time that nothing is a time of.
    /// </remarks>
    private void PublishMoment()
    {
        if (_showing)
        {
            return;
        }

        if (_day?.SelectedDate is not { } day)
        {
            Value = string.Empty;
            return;
        }

        var moment = day.Date.Add(_time?.SelectedTime ?? TimeSpan.Zero);

        Value = moment.ToString(MomentFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Hands the box's text back exactly as it stands.
    /// </summary>
    private void PublishText()
    {
        if (_showing)
        {
            return;
        }

        Value = _text?.Text ?? string.Empty;
    }
}
