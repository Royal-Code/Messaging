using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     A base component to communicate with RabbitMQ.
/// </para>
/// </summary>
public abstract class BaseComponent : IDisposable
{
    private readonly ManagedChannel managedChannel;
    private readonly EventHandler<bool> onReconnected;
    private bool disposedValue;

    public BaseComponent(IChannelManager channelManager, ChannelStrategy channelStrategy)
    {
        managedChannel = channelStrategy switch
        {
            ChannelStrategy.Exclusive => channelManager.CreateChannel(),
            ChannelStrategy.Shared => channelManager.GetSharedChannel(),
            ChannelStrategy.Pooled => channelManager.GetPooledChannel(),
            _ => throw new CommunicationException($"Invalid channel strategy: {channelStrategy}")
        };
        onReconnected = OnReconnected;
        managedChannel.OnReconnected += onReconnected;
    }

    /// <summary>
    /// Get the current channel.
    /// </summary>
    protected IModel? CurrentModel
    {
        get
        {

        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                managedChannel.Dispose();
                currentModel?.Dispose();
                currentModel = null;
            }

            disposedValue = true;
        }
    }

    private void OnReconnected(object? sender, bool e)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
