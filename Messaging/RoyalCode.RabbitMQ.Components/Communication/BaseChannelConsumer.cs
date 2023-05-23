using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     A base component to communicate with RabbitMQ.
/// </para>
/// <para>
///     This component handles the complexities of maintaining a channel with RabbitMQ 
///     and recreating channels when the connection is re-established.
/// </para>
/// </summary>
public abstract class BaseChannelConsumer : IChannelConsumer, IDisposable
{
    private readonly ChannelStrategy channelStrategy;
    private readonly IChannelConsumerStatus status;
    
    private bool initialized;
    private bool disposedValue;
    private bool connectionIsClosed = true;

    private IModel? currentModel;

    /// <summary>
    /// Base constructor, consume the channel provider.
    /// </summary>
    /// <param name="factory">Factory for channels managers.</param>
    /// <param name="clusterName">The RabbitMQ cluster name, used to get connections.</param>
    /// <param name="channelStrategy">The strategy to consume channels.</param>
    protected BaseChannelConsumer(IChannelManagerFactory factory, string clusterName, ChannelStrategy channelStrategy)
        :this(factory.GetChannelManager(clusterName), channelStrategy)
    { }

    protected BaseChannelConsumer(IChannelManager channelManager, ChannelStrategy channelStrategy)
    {
        initialized = false;

        this.channelStrategy = channelStrategy;

        ManagedChannel managedChannel = channelStrategy switch
        {
            ChannelStrategy.Exclusive => channelManager.CreateChannel(),
            ChannelStrategy.Shared => channelManager.GetSharedChannel(),
            _ => channelManager.GetPooledChannel(),
        };

        status = managedChannel.Consume(this);

        initialized = true;
    }

    /// <summary>
    /// Protected property to informe super classes if the connection is closed or not.
    /// </summary>
    protected bool ConnectionIsClosed => connectionIsClosed;

    /// <summary>
    /// <para>
    ///     Return the RabbitMQ chanell to the pool.
    /// </para>
    /// </summary>
    /// <param name="model">The used channel.</param>
    protected void ReturnChannelToPublish(IModel model)
    {
        GuardConnected();

        if (channelStrategy is ChannelStrategy.Pooled)
            provider!.ReturnPooledChannel(model);
    }

    private void GuardConnected()
    {
        if (provider is null || connectionIsClosed)
            throw new CommunicationException("Connection with RabbitMQ is closed");
    }

    /// <summary>
    /// <para>
    ///     After get a channel, exclusive or shared, and the connection is closed e reconnected,
    ///     this method will be called.
    /// </para>
    /// </summary>
    /// <param name="autorecovered">If the connection was auto recovered.</param>
    protected abstract void ConnectionIsRecovered(bool autorecovered);

    /// <summary>
    /// Release resources.
    /// </summary>
    /// <param name="disposing">If is disposing, called by <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                status.Dispose();
                if (currentModel is not null && channelStrategy == ChannelStrategy.Exclusive)
                    currentModel.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
    }

    public void Consume(IModel channel)
    {
        throw new NotImplementedException();
    }

    public void ConnectionRecovered(IModel channel, bool autorecovered)
    {
        throw new NotImplementedException();
    }

    public void ChannelRecovered(IModel channel)
    {
        throw new NotImplementedException();
    }

    public void Disposing()
    {
        throw new NotImplementedException();
    }
}