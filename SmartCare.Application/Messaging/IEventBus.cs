namespace SmartCare.Application.Messaging
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event);
        void Subscribe<TEvent>(Func<TEvent, Task> handler);
    }
}