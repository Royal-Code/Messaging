using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Manage a channel (<see cref="IModel"/>) of the RabbitMQ.
/// </para>
/// </summary>
public abstract class ManagedChannel
{
    /// <summary>
    /// Check if the channel is open.
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    /// <para>
    ///     Get the channel (<see cref="IModel"/>) of the RabbitMQ.
    /// </para>
    /// <para>
    ///     It can be null if the connection is not established.
    /// </para>
    /// </summary>
    public IModel? Channel { get; }

    /// <summary>
    /// <para>
    ///     Consume a channel of RabbitMQ, object of type <see cref="IModel"/>.
    /// </para>
    /// </summary>
    /// <param name="consumer">The channel consumer.</param>
    /// <returns>A <ver cref="IDisposable"/> object to finalize the consumption.</returns>
    IChannelConsumerStatus Consume(IChannelConsumer consumer)
    {
        throw new NotImplementedException();
    }

    void Release()
    {
        throw new NotImplementedException();
    }
}