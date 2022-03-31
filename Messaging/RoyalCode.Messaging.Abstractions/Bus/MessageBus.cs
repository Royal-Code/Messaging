namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// <para>
///     Default implementation of <see cref="IMessageBus"/> that use the <see cref="PublisherFactory"/>
///     and <see cref="ReceiverFactory"/> to create publishers e receivers.
/// </para>
/// </summary>
public class MessageBus : IMessageBus
{
    private readonly PublisherFactory factory;

    /// <summary>
    /// Creates a new message bus.
    /// </summary>
    /// <param name="factory"></param>
    public MessageBus(PublisherFactory factory)
    {
        this.factory = factory;
    }
    
    public IPublisher<TMessage> CreatePublisher<TMessage>()
    {
        var publisher = factory.Create<TMessage>();

        if (publisher is not null)
            return publisher;

        // create a new exception of type PublisherNotConfiguredException
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