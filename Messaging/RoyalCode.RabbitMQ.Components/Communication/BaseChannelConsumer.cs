using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     A base component to communicate with RabbitMQ.
/// </para>
/// <para>
///     This component consume the channel, so it will be notified when the channel is re-established.
/// </para>
/// </summary>
public abstract class BaseChannelConsumer : BaseComponent, IChannelConsumer
{
    private readonly IChannelConsumerStatus status;

    /// <summary>
    /// Construtor of the base component, consuming the channel.
    /// </summary>
    /// <param name="channelManager"></param>
    /// <param name="channelStrategy"></param>
    protected BaseChannelConsumer(IChannelManager channelManager, ChannelStrategy channelStrategy)
        :base(channelManager, channelStrategy)
    {
        status = Managed.Consume(this);
    }

    /// <inheritdoc />
    public abstract void Consume(IModel channel);

    /// <inheritdoc />
    public abstract void Reloaded(IModel channel, bool autorecovered);

    /// <summary>
    /// Dispose the component.
    /// </summary>
    public void Disposing()
    {
        Dispose();
    }

    /// <summary>
    /// Since this object is a channel consumer, it will be notified when the channel is re-established.
    /// So, is not necessary to do anything here.
    /// </summary>
    protected override void OnReconnected(object? sender, bool autorecovered) { }

    /// <summary>
    /// Check if the channel strategy is valid. The pooled strategy is not allowed for receivers.
    /// </summary>
    /// <param name="channelStrategy">The current strategy.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is pooled.
    /// </exception>
    protected override void GuardChannelStrategy(ChannelStrategy channelStrategy)
    {
        if (channelStrategy == ChannelStrategy.Pooled)
            throw new CommunicationException(
                "Pooled Channel Strategy is not allowed for receivers. The receivers will not realease the channels.");
    }

    /// <summary>
    /// Release resources.
    /// </summary>
    /// <param name="disposing">If is disposing.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            status.Dispose();
        }

        base.Dispose(disposing);
    }
}