using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     A base component to communicate with RabbitMQ.
/// </para>
/// </summary>
public abstract class BaseComponent : IDisposable
{
    private readonly EventHandler<bool> onReconnected;
    private bool disposedValue;

    /// <summary>
    /// Base constructor, create the managed channel.
    /// </summary>
    /// <param name="channelManager"></param>
    /// <param name="channelStrategy"></param>
    /// <exception cref="CommunicationException"></exception>
    protected BaseComponent(IChannelManager channelManager, ChannelStrategy channelStrategy)
    {
        Managed = channelStrategy switch
        {
            ChannelStrategy.Exclusive => channelManager.CreateChannel(),
            ChannelStrategy.Shared => channelManager.GetSharedChannel(),
            ChannelStrategy.Pooled => channelManager.GetPooledChannel(),
            _ => throw new CommunicationException($"Invalid channel strategy: {channelStrategy}")
        };
        onReconnected = OnReconnected;
        Managed.OnReconnected += onReconnected;
    }

    /// <summary>
    /// The managed channel.
    /// </summary>
    protected ManagedChannel Managed { get; }

    /// <summary>
    /// Dispose the managed channel.
    /// </summary>
    /// <param name="disposing">If the managed channel should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Managed.OnReconnected -= onReconnected;
                Managed.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Possibility to execute some action when the connection or channel is re-established.
    /// </summary>
    /// <param name="sender">The sender object, that is the managed channel.</param>
    /// <param name="autorecovered">If the reconnection is due to an auto-recovery.</param>
    protected abstract void OnReconnected(object? sender, bool autorecovered);

    /// <summary>
    /// Check if the current strategy is safe for the component operations.
    /// </summary>
    /// <param name="channelStrategy"></param>
    protected abstract void GuardChannelStrategy(ChannelStrategy channelStrategy);

    /// <summary>
    /// Dispose the managed channel.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
