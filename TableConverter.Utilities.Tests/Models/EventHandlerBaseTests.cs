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
    public void UnsubscribeAll_Removes_A_Lambda_That_Captures_Only_Its_Owner()
    {
        // A lambda that captures only `this` needs no closure, so the compiler makes it a private
        // instance method and the delegate's target is the owner itself. That is what lets UnsubscribeAll
        // find it: the target it compares against is the owner.
        var testEvent = new TestEvent();
        var owner = new SelfSubscribing();

        owner.SubscribeTo(testEvent);
        testEvent.Publish(EventArgs.Empty);
        Assert.Equal(1, owner.Count);

        testEvent.UnsubscribeAll(owner);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, owner.Count);
    }

    [Fact]
    public void UnsubscribeAll_Does_Not_Remove_A_Lambda_That_Captures_Anything_Else()
    {
        // A lambda that captures a local needs a closure, so the compiler hoists it into a display class
        // and the delegate's target is that class - not the object the lambda mentions. UnsubscribeAll
        // compares targets, so it does not match and the handler stays. Registering through an
        // EventRegistrar avoids this: it removes the handler it was handed rather than searching for it.
        var testEvent = new TestEvent();
        var owner = new Subscriber();

        testEvent.Subscribe((_, args) => owner.OnEvent(null, args));

        testEvent.UnsubscribeAll(owner);
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(1, owner.Count);
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
    public void Publish_Ignores_A_Lambda_Whose_Closure_Was_Collected()
    {
        // Handlers are held weakly, so a lambda subscribed straight to an event stops being notified
        // once nothing else holds its delegate. A lambda that captures only the object it lives on is
        // kept alive by that object and hides this; one that captures anything else does not, so a
        // subscription worth keeping belongs on an EventRegistrar, which holds its delegate.
        var testEvent = new TestEvent();
        var subscriber = new Subscriber();

        SubscribeCollectibleLambda(testEvent, subscriber);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Must not throw; the collected lambda is simply no longer there.
        testEvent.Publish(EventArgs.Empty);

        Assert.Equal(0, subscriber.Count);
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

        Task.WaitAll([publisher, .. mutators]);

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
    public void EventManager_Returns_The_Same_Instance_For_The_Same_Event_TYPE()
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SubscribeCollectibleLambda(TestEvent testEvent, Subscriber subscriber)
    {
        // Captures only the subscriber passed in, so nothing but the weak handler refers to the closure.
        testEvent.Subscribe((_, args) => subscriber.OnEvent(null, args));
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

    /// <summary>
    /// Subscribes a lambda that captures only <see langword="this" />, which is the shape the compiler
    /// turns into an instance method rather than a closure.
    /// </summary>
    private sealed class SelfSubscribing
    {
        public int Count { get; private set; }

        public void SubscribeTo(TestEvent testEvent)
        {
            testEvent.Subscribe((_, _) => Count++);
        }
    }
}
