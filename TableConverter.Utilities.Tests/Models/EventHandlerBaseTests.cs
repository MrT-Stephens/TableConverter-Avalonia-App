using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Models;

public class EventHandlerBaseTests
{
    [Fact]
    public void Publish_Invokes_All_Subscribed_Handlers()
    {
        var testEvent = new TestEvent();
        var first = new Subscriber();
        var second = new Subscriber();

        testEvent.Subscribe(first.OnEvent);
        testEvent.Subscribe(second.OnEvent);

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void Publish_Does_Nothing_When_Nobody_Subscribed()
    {
        var testEvent = new TestEvent();

        testEvent.Publish(EventArgs.Empty);
    }

    [Fact]
    public void Unsubscribe_Stops_Further_Invocation()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();

        testEvent.Subscribe(subscriber.OnEvent);
        testEvent.Publish(EventArgs.Empty);
        testEvent.Unsubscribe(subscriber.OnEvent);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void Unsubscribe_On_A_Missing_Handler_Is_A_NoOp()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();

        testEvent.Unsubscribe(subscriber.OnEvent);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, subscriber.Count);
    }

    [Fact]
    public void UnsubscribeAll_Removes_Only_That_Subscribers_Handlers()
    {
        // Regression: UnsubscribeAll used to throw InvalidOperationException ("collection was modified").
        var testEvent = new TestEvent();
        var first = new Subscriber();
        var second = new Subscriber();

        testEvent.Subscribe(first.OnEvent);
        testEvent.Subscribe(second.OnEvent);

        testEvent.UnsubscribeAll(first);

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void UnsubscribeAll_Does_Nothing_For_An_Unknown_Subscriber()
    {
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();

        testEvent.Subscribe(subscriber.OnEvent);
        testEvent.UnsubscribeAll(new Subscriber());

        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void Publish_Allows_A_Handler_To_Unsubscribe_During_Notification()
    {
        // Regression: unsubscribing from inside a handler used to invalidate the publishing loop.
        var testEvent = new TestEvent();

        Subscriber? subscriber = null;
        subscriber = new Subscriber(() => testEvent.Unsubscribe(subscriber!.OnEvent));

        testEvent.Subscribe(subscriber.OnEvent);

        testEvent.Publish(EventArgs.Empty);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, subscriber.Count);
    }

    [Fact]
    public void Publish_Allows_A_Handler_To_Subscribe_During_Notification()
    {
        var testEvent = new TestEvent();
        var late = new Subscriber();

        Subscriber? subscriber = null;
        subscriber = new Subscriber(() => testEvent.Subscribe(late.OnEvent));

        testEvent.Subscribe(subscriber.OnEvent);

        // Snapshot semantics: a handler added while publishing is notified from the next publish onwards.
        testEvent.Publish(EventArgs.Empty);
        Assert.Equal(0, late.Count);

        testEvent.Publish(EventArgs.Empty);
        Assert.Equal(1, late.Count);
    }

    [Fact]
    public void Publish_Ignores_Handlers_Whose_Subscriber_Was_Collected()
    {
        var testEvent = new TestEvent();

        SubscribeCollectibleSubscriber(testEvent);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var live = new Subscriber();
        testEvent.Subscribe(live.OnEvent);

        // Must not throw and must still notify the live subscriber.
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, live.Count);
    }

    [Fact]
    public async Task Concurrent_Subscription_Changes_Do_Not_Break_Publishing()
    {
        // Regression: Publish used to iterate the handler list without taking the lock,
        // so a concurrent (un)subscribe produced "Collection was modified" exceptions.
        var testEvent = new TestEvent();
        var subscribers = Enumerable.Range(0, 4).Select(_ => new Subscriber()).ToArray();
        var errors = new ConcurrentBag<Exception>();

        var publisher = Task.Run(() =>
        {
            for (var i = 0; i < 2_000; i++)
            {
                try
                {
                    testEvent.Publish(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        });

        var mutators = subscribers.Select((subscriber, index) => Task.Run(() =>
        {
            for (var i = 0; i < 500; i++)
            {
                try
                {
                    if ((i + index) % 2 == 0)
                    {
                        testEvent.Subscribe(subscriber.OnEvent);
                    }
                    else
                    {
                        testEvent.Unsubscribe(subscriber.OnEvent);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(publisher, Task.WhenAll(mutators));

        Assert.Empty(errors);
    }

    [Fact]
    public void Subscribe_Throws_For_Null()
    {
        var testEvent = new TestEvent();

        Assert.Throws<ArgumentNullException>(() => testEvent.Subscribe(null!));
        Assert.Throws<ArgumentNullException>(() => testEvent.Unsubscribe(null!));
        Assert.Throws<ArgumentNullException>(() => testEvent.UnsubscribeAll(null!));
    }

    [Fact]
    public void Every_Event_Has_A_Unique_Identifier()
    {
        Assert.NotEqual(new TestEvent().EventId, new TestEvent().EventId);
    }

    [Fact]
    public void EventManager_UnregisterAllEvents_Removes_Only_That_Subscribers_Handlers()
    {
        var eventManager = new EventManager();

        var firstEvent = eventManager.GetEvent<TestEvent>();
        var secondEvent = eventManager.GetEvent<SecondTestEvent>();

        var first = new Subscriber();
        var second = new Subscriber();

        firstEvent.Subscribe(first.OnEvent);
        firstEvent.Subscribe(second.OnEvent);
        secondEvent.Subscribe(first.OnEvent);

        eventManager.UnregisterAllEvents(first);

        firstEvent.Publish(EventArgs.Empty);
        secondEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void EventManager_Returns_The_Same_Instance_For_The_Same_Event_Type()
    {
        var eventManager = new EventManager();

        Assert.Same(eventManager.GetEvent<TestEvent>(), eventManager.GetEvent<TestEvent>());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SubscribeCollectibleSubscriber(TestEvent testEvent)
    {
        var collected = new Subscriber();
        testEvent.Subscribe(collected.OnEvent);
    }

    private sealed class TestEvent : EventHandlerBase<EventArgs>;

    private sealed class SecondTestEvent : EventHandlerBase<EventArgs>;

    private sealed class Subscriber
    {
        private readonly Action? _onEvent;

        public Subscriber(Action? onEvent = null)
        {
            _onEvent = onEvent;
        }

        public int Count { get; private set; }

        public void OnEvent(object? sender, EventArgs args)
        {
            Count++;
            _onEvent?.Invoke();
        }
    }
}

