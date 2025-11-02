namespace SmartCare.Application.commons
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event);
        void Subscribe<TEvent>(Func<TEvent, Task> handler);
    }
}