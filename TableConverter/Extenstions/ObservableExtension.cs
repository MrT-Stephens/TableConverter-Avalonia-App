using Avalonia.Reactive;
using System;

namespace TableConverter.Extenstions;

public static class ObservableExtension
{
    public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> action)
    {
        return observable.Subscribe(new AnonymousObserver<T>(action));
    }
}