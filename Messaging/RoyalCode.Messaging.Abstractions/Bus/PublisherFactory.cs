namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Factory to create <see cref="IPublisher{TMessage}"/> through <see cref="IPublisherProvider"/>.
/// </summary>
public class PublisherFactory // may be ChannelFactory and IChannelProvider for create publishers and receivers ?
{
    private readonly IEnumerable<IPublisherProvider> providers;

    /// <summary>
    /// Creates a new factory with the providers.
    /// </summary>
    /// <param name="providers">The many (or only one) publishers providers.</param>
    public PublisherFactory(IEnumerable<IPublisherProvider> providers)
    {
        this.providers = providers;
    }
    
    /// <summary>
    /// <para>
    ///     Try creating a publisher for the message.
    /// </para>
    /// <para>
    ///     If there is a publish configuration for any broker,
    ///     the publisher will be created,
    ///     otherwise it will be null.
    /// </para>
    /// <para>
    ///     If there are configurations for more than one broker,
    ///     an internal publisher will be created,
    ///     which will delegate the message to the publishers of the various brokers.
    /// </para>
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <returns>A publisher or null if the message type is not configured.</returns>
    public IPublisher<TMessage>? Create<TMessage>()
    {
        IPublisher<TMessage>? publisher = null;
        MultiPublisher<TMessage>? multiPublisher = null;

        foreach (var provider in providers)
        {
            var current = provider.FindPublisher<TMessage>();
            if (current is null)
                continue;

            if (publisher is null)
            {
                publisher = current;
                continue;
            }

            if (multiPublisher is null)
            {
                multiPublisher = new MultiPublisher<TMessage>();
                multiPublisher.AddPublisher(publisher);
                publisher = multiPublisher;
            }
            
            multiPublisher.AddPublisher(current);
        }

        return publisher;
    }
}