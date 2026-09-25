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
}
