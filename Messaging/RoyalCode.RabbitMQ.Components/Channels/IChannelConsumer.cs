namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Interface for components that consume RabbitMQ channels for publishing or receiving messages.
/// </para>
/// <para>
///     This interface is used by the <see cref="ChannelManager"/>.
/// </para>
/// </summary>
public interface IChannelConsumer
{
    /// <summary>
    /// Assigns the <see cref="IChannelProvider"/> to be consumed by the component.
    /// </summary>
    /// <param name="provider">The <see cref="IChannelProvider"/> to access the channels.</param>
    void Consume(IChannelProvider provider);

    /// <summary>
    /// Informs when the connection has been closed, usually due to some error or failure.
    /// </summary>
    void ConnectionClosed();

    /// <summary>
    /// Informs when the connection has been re-established.
    /// </summary>
    /// <param name="autorecovered">If the connection was auto recovered.</param>
    void ConnectionRecovered(bool autorecovered);
}