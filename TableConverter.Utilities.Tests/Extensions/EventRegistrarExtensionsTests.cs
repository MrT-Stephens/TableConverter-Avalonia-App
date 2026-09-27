using System.Runtime.CompilerServices;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Extensions;

public class EventRegistrarExtensionsTests
{
    [Fact]
    public void RegisterSubscription_Subscribes_Immediately()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        using var registrar = new EventRegistrar();

        registrar.RegisterSubscription(testEvent, subscriber.OnEvent);

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void Disposing_The_Registrar_Removes_The_Subscription()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        var registrar = new EventRegistrar();

        registrar.RegisterSubscription(testEvent, subscriber.OnEvent);

        testEvent.Publish(EventArgs.Empty);
        registrar.Dispose();
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void Clear_Removes_Only_The_Subscriptions_Of_That_Owner()
    {
        var testEvent = new TestEvent();
        var first = new Subscriber();
        var second = new Subscriber();
        using var registrar = new EventRegistrar();

        var firstOwner = new object();

        registrar.RegisterSubscription(testEvent, first.OnEvent, firstOwner);
        registrar.RegisterSubscription(testEvent, second.OnEvent, new object());

        registrar.Clear(firstOwner);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void ClearAll_Removes_Subscriptions_Registered_Without_An_Owner()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        using var registrar = new EventRegistrar();

        registrar.RegisterSubscription(testEvent, subscriber.OnEvent);

        registrar.ClearAll();
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, subscriber.Count);
    }

    [Fact]
    public void Disposing_A_Registration_Removes_Only_That_Subscription()
    {
        var testEvent = new TestEvent();
        var first = new Subscriber();
        var second = new Subscriber();
        using var registrar = new EventRegistrar();

        var registration = registrar.RegisterSubscription(testEvent, first.OnEvent);
        registrar.RegisterSubscription(testEvent, second.OnEvent);

        registration.Dispose();
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void RegisterSubscription_Does_Not_Duplicate_Handlers()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        using var registrar = new EventRegistrar();

        registrar.RegisterSubscription(testEvent, subscriber.OnEvent);

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void RegisterSubscription_Throws_For_Null_Arguments()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        using var registrar = new EventRegistrar();

        Assert.Throws<ArgumentNullException>(() =>
            registrar.RegisterSubscription<EventArgs>(null!, subscriber.OnEvent));

        Assert.Throws<ArgumentNullException>(() =>
            registrar.RegisterSubscription(testEvent, null!));
    }

    private sealed class TestEvent : EventHandlerBase<EventArgs>;

    private sealed class Subscriber
    {
        public int Count { get; private set; }

        public void OnEvent(object? sender, EventArgs args)
        {
            Count++;
        }
    }

    #region Lambda handlers

    /// <summary>
    /// A plain C# event, which is what <c>RegisterEvent</c> is for: an event on a control or an object the
    /// registerer does not own, rather than one of the application's own events.
    /// </summary>
    /// <remarks>
    /// The event is written out rather than left as a field-like event so the subscribed delegate can be
    /// read, which is what lets a test tell a handler that has been taken off from one that has merely
    /// stopped being called.
    /// </remarks>
    private sealed class Source
    {
        private EventHandler? _changed;

        public event EventHandler? Changed
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        /// <summary>
        /// The delegate currently subscribed, or <see langword="null" /> when nobody is.
        /// </summary>
        public EventHandler? Subscribed => _changed;

        public void Raise() => _changed?.Invoke(this, EventArgs.Empty);
    }

    [Fact]
    public void RegisterEvent_Subscribes_A_Lambda_Handler_Immediately()
    {
        var source = new Source();
        var calls = 0;
        using var registrar = new EventRegistrar();

        registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            null,
            (_, _) => calls++);

        source.Raise();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Disposing_The_Registrar_Removes_A_Lambda_Handler()
    {
        var source = new Source();
        var calls = 0;
        var registrar = new EventRegistrar();

        registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            null,
            (_, _) => calls++);

        source.Raise();
        registrar.Dispose();
        source.Raise();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void ClearAll_Removes_A_Lambda_Handler()
    {
        var source = new Source();
        var calls = 0;
        using var registrar = new EventRegistrar();

        registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            null,
            (_, _) => calls++);

        source.Raise();
        registrar.ClearAll();
        source.Raise();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Clear_Removes_A_Lambda_Handler_Of_That_Owner_Only()
    {
        var source = new Source();
        var first = 0;
        var second = 0;
        using var registrar = new EventRegistrar();

        var firstOwner = new object();

        registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            firstOwner,
            (_, _) => first++);

        registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            new object(),
            (_, _) => second++);

        source.Raise();
        registrar.Clear(firstOwner);
        source.Raise();

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void Disposing_A_Registration_Removes_Just_That_Lambda_Handler()
    {
        // A registration to dispose of on its own comes from Register, because RegisterEvent hands back
        // the registrar so its calls can be chained. What RegisterEvent builds inside is exactly this.
        var source = new Source();
        var first = 0;
        var second = 0;
        using var registrar = new EventRegistrar();

        var registration = registrar.Register(new EventHandle<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            (_, _) => first++));

        registrar.Register(new EventHandle<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            (_, _) => second++));

        source.Raise();
        registration.Dispose();
        source.Raise();

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void A_Lambda_Handler_Comes_Off_Exactly_As_It_Went_On()
    {
        // The extension is handed one handler and passes that same instance to both the add and the
        // remove, so what comes off is what went on. Writing the handler out twice instead - once in the
        // add and once in the remove - would compile to two different methods which do not match, and the
        // handler would be left subscribed forever. This is why RegisterEvent takes the handler itself
        // rather than an add and a remove that each name one.
        var source = new Source();
        using var registrar = new EventRegistrar();

        var registration = registrar.Register(new EventHandle<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            (_, _) => { }));

        Assert.NotNull(source.Subscribed);

        registration.Dispose();

        Assert.Null(source.Subscribed);
    }

    [Fact]
    public void RegisterEvent_Hands_Back_The_Registrar_So_Calls_Can_Be_Chained()
    {
        // What RegisterEvent returns is the registrar, not the registration for the one handler, so that
        // calls can be chained. Disposing what it returns therefore releases every handler the registrar
        // holds, which reads like disposing a single registration. RegisterSubscription is the one that
        // hands back a registration.
        var source = new Source();
        using var registrar = new EventRegistrar();

        var returned = registrar.RegisterEvent<EventHandler>(
            action => source.Changed += action,
            action => source.Changed -= action,
            null,
            (_, _) => { });

        Assert.Same(registrar, returned);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SubscribeLambda(TestEvent testEvent, IEventRegistrar registrar, Subscriber subscriber)
    {
        // Captures only the subscriber, so nothing but the registration refers to the closure.
        registrar.RegisterSubscription(testEvent, (_, args) => subscriber.OnEvent(null, args));
    }

    [Fact]
    public void RegisterSubscription_Keeps_A_Capturing_Lambda_Subscribed()
    {
        // A subscription made through a registrar holds its delegate, so a lambda is not collectible
        // while the registration stands. Subscribing the same lambda directly does not have this
        // protection: see EventHandlerBaseTests.
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();
        using var registrar = new EventRegistrar();

        SubscribeLambda(testEvent, registrar, subscriber);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    #endregion
}