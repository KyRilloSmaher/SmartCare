using Microsoft.Extensions.Logging;
using SmartCare.Application.Messaging;
using System.Collections.Concurrent;

namespace SmartCare.InfraStructure.Messaging
{
 

    public class InMemoryEventBus : IEventBus
    {
        private readonly ConcurrentBag<Func<object, Task>> _handlers = new();
        private readonly ILogger<InMemoryEventBus> _logger;

        public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
        {
            _logger = logger;
        }

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
            _logger.LogDebug("-------------------------PUBLISH Event ASYNC ---------------------");
            foreach (var handler in _handlers)
            {
                try
                {
                    _logger.LogDebug($"-------------------------{nameof(handler.GetType)}is Activeted Now ---------------------");
                    await handler(@event);
                    _logger.LogDebug($"-------------------------Event handler {nameof(handler.GetType)} is Finished Successfully : ) -------------------------");
                }
                catch (Exception ex)
                {
                    // Log or handle handler exceptions safely
                    _logger.LogError($"-------------------------Event handler {nameof(handler.GetType)} Failed : ( -------------------------");
                    _logger.LogError($" Error : {ex.Message}");
                    _logger.LogError("--------------------------------------------------");
                }
            }
        }
    }

}
