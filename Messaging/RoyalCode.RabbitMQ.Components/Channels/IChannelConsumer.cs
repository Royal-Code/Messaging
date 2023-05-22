using RabbitMQ.Client;

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
    /// Assigns the <see cref="IModel"/> to be consumed by the component.
    /// </summary>
    /// <param name="channel">The <see cref="IModel"/> to access the channels.</param>
    void Consume(IModel channel);

    /// <summary>
    /// Informs when the connection has been re-established.
    /// </summary>
    /// <param name="channel">
    ///     The <see cref="IModel"/> to access the channels.
    ///     If the connection was not auto recovered, this will be a new channel.
    /// </param>
    /// <param name="autorecovered">If the connection was auto recovered.</param>
    void ConnectionRecovered(IModel channel, bool autorecovered);

    /// <summary>
    /// <para>
    ///     Informs when the channel has been re-established,
    ///     but not when the connection is auto recovered or re-established.
    ///     This are called when the channel is closed and then re-opened/re-created.
    /// </para>
    /// </summary>
    /// <param name="channel">The <see cref="IModel"/> to access the channels.</param>
    void ChannelRecovered(IModel channel);

    /// <summary>
    /// <para>
    ///     Informs when the connection is disposing and the channel is closing.
    /// </para>
    /// </summary>
    void Disposing();
}