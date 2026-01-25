using System;
using TableConverter.Utilities.Models;

namespace TableConverter.Contracts.Events;

public class PageNavigationRequestedEventArgs : EventArgs
{
    public required string ViewModelName { get; set; }

    public Action<object?>?  Action { get; set; }
}

public sealed class PageNavigationRequestedEvent : EventHandlerBase<PageNavigationRequestedEventArgs>
{
}