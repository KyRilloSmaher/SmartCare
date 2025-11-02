using SmartCare.Application.commons;
using System.Collections.Concurrent;

namespace SmartCare.API.Events
{
 

    public class InMemoryEventBus : IEventBus
    {
        private readonly ConcurrentBag<Func<object, Task>> _handlers = new();

        public void Subscribe<TEvent>(Func<TEvent, Task> handler)
        {
            _handlers.Add(async e =>
            {
                if (e is TEvent evt)
                    await handler(evt);
            });
        }

        public async Task PublishAsync<TEvent>(TEvent @event)
        {
            foreach (var handler in _handlers)
            {
                try
                {
                    await handler(@event);
                }
                catch (Exception ex)
                {
                    // Log or handle handler exceptions safely
                    Console.WriteLine($"Event handler error: {ex.Message}");
                }
            }
        }
    }

}
