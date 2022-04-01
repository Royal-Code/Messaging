using RoyalCode.Messaging.Abstractions.Providers;

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Factory to create <see cref="IReceiver{TMessage}"/> through <see cref="IReceiverProvider"/>.
/// </summary>
public class ReceiverFactory
{
    private readonly IEnumerable<IReceiverProvider> providers;

    /// <summary>
    /// Creates a new factory with the providers.
    /// </summary>
    /// <param name="providers">The many (or only one) publishers providers.</param>
    public ReceiverFactory(IEnumerable<IReceiverProvider> providers)
    {
        this.providers = providers;
    }

    /// <summary>
    /// <para>
    ///     Try creating a receiver for the message.
    /// </para>
    /// <para>
    ///     If there is a receiver configuration for any broker,
    ///     the receiver will be created,
    ///     otherwise it will be null.
    /// </para>
    /// <para>
    ///     If there are configurations for more than one broker,
    ///     an internal receiver will be created,
    ///     which will delegate the listeners to the receivers of the various brokers.
    /// </para>
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <returns>A receiver or null if the message type is not configured.</returns>
    public IReceiver<TMessage>? Create<TMessage>()
    {
        IReceiver<TMessage>? receiver = null;
        MultiReceiver<TMessage>? multiReceiver = null;

        foreach (var provider in providers)
        {
            var current = provider.FindReceiver<TMessage>();
            if (current is null)
                continue;

            if (receiver is null)
            {
                receiver = current;
                continue;
            }

            if (multiReceiver is null)
            {
                multiReceiver = new MultiReceiver<TMessage>();
                multiReceiver.AddReceiver(receiver);
                receiver = multiReceiver;
            }

            multiReceiver.AddReceiver(current);
        }

        return receiver;
    }
}
