namespace RoyalCode.Messaging.Abstractions.Bus;

public class MessageBus : IMessageBus
{
    public IPublisher<TMessage> CreatePublisher<TMessage>()
    {
        throw new NotImplementedException();
    }

    public IReceiver<TMessage> CreateReceiver<TMessage>()
    {
        throw new NotImplementedException();
    }

    public Task Publish<TMessage>(TMessage instance)
    {
        throw new NotImplementedException();
    }
}