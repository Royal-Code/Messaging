using Microsoft.Extensions.Options;
using RoyalCode.Messaging.Abstractions.Exceptions;
using RoyalCode.Messaging.Abstractions.Options;

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// <para>
///     Default implementation of <see cref="IMessageBus"/> that use the <see cref="PublisherFactory"/>
///     and <see cref="ReceiverFactory"/> to create publishers e receivers.
/// </para>
/// </summary>
public class MessageBus : IMessageBus
{
    private readonly Lazy<PublisherFactory> publisherFactory;
    private readonly Lazy<ReceiverFactory> receiverFactory;
    private readonly IOptions<MessagingOptions> options;

    /// <summary>
    /// Creates a new message bus.
    /// </summary>
    /// <param name="publisherFactory">The publisher factory.</param>
    /// <param name="receiverFactory">The receiver factory.</param>
    /// <param name="options">Options.</param>
    public MessageBus(
        Lazy<PublisherFactory> publisherFactory,
        Lazy<ReceiverFactory> receiverFactory,
        IOptions<MessagingOptions> options)
    {
        this.publisherFactory = publisherFactory;
        this.receiverFactory = receiverFactory;
        this.options = options;
    }
    
    /// <inheritdoc />
    public IPublisher<TMessage> CreatePublisher<TMessage>()
    {
        var publisher = publisherFactory.Value.Create<TMessage>();

        if (publisher is not null)
            return publisher;

        if (options.Value.ThrowIfNotFoundPublisherOrReceiver)
            throw new PublisherNotConfiguredException(typeof(TMessage));

        return new NoPublisher<TMessage>();
    }

    /// <inheritdoc />
    public IReceiver<TMessage> CreateReceiver<TMessage>()
    {
        var receiver = receiverFactory.Value.Create<TMessage>();

        if (receiver is not null)
            return receiver;
        
        if (options.Value.ThrowIfNotFoundPublisherOrReceiver)
            throw new ReceiverNotConfiguredException(typeof(TMessage));

        return new NoReceiver<TMessage>();
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage instance, CancellationToken token = default)
        => CreatePublisher<TMessage>().PublishAsync(instance, token);
}