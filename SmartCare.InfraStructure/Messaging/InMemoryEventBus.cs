using SmartCare.Application.Messaging;
using System.Collections.Concurrent;

namespace SmartCare.InfraStructure.Messaging
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
            Console.WriteLine("-------------------------PUBLISH  ASYNC ---------------------");
            foreach (var handler in _handlers)
            {
                try
                {
                    Console.WriteLine("-------------------------handler is not Null  ASYNC ---------------------");
                    await handler(@event);
                    Console.WriteLine($"Event handler ");
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
