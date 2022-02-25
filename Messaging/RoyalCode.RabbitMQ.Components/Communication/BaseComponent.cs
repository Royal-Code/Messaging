using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using System;
using System.Threading;
using System.Threading.Tasks;

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
public abstract class BaseComponent : IChannelConsumer, IDisposable
{
    private readonly ChannelStrategy channelStrategy;
    private readonly IChannelConsummation consummation;
    
    private bool initialized;
    private bool disposedValue;
    private bool connectionIsClosed = true;

    private IChannelProvider? provider;
    private IModel? currentModel;

    /// <summary>
    /// Base constructor, consume the channel provider.
    /// </summary>
    /// <param name="channelManager">Component to consume channels.</param>
    /// <param name="clusterName">The RabbitMQ cluster name, used to get connections.</param>
    /// <param name="channelStrategy">The strategy to consume channels.</param>
    protected BaseComponent(IChannelManager channelManager, string clusterName, ChannelStrategy channelStrategy)
    {
        initialized = false;

        this.channelStrategy = channelStrategy;
        consummation = channelManager.Consume(clusterName, this);
        
        initialized = true;
    }

    /// <summary>
    /// Protected property to informe super classes if the connection is closed or not.
    /// </summary>
    protected bool ConnectionIsClosed => connectionIsClosed;

    /// <summary>
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>, for receive messages.
    /// </summary>
    /// <returns></returns>
    protected IModel GetChannelToReceive()
    {
        GuardConnected();

        return GetCurrentModel();
    }

    /// <summary>
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>, to publish messages.
    /// </summary>
    protected Task<IModel> GetChannelToPublishAsync(CancellationToken cancellationToken)
    {
        GuardConnected();

        return channelStrategy switch
        {
            ChannelStrategy.Pooled => provider!.GetPooledChannelAsync(cancellationToken),
            _                      => Task.FromResult(currentModel ??= GetCurrentModel())
        };
    }

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

    private IModel GetCurrentModel() => currentModel ??= channelStrategy switch
    {
        ChannelStrategy.Exclusive => provider!.CreateChannel(),
        ChannelStrategy.Shared => provider!.GetSharedChannel(),
        _ => throw new InvalidOperationException("Is not possible get current model (channel) from a pool")
    };

    private void GuardConnected()
    {
        if (provider is null || connectionIsClosed)
            throw new CommunicationException("Connection with RabbitMQ is closed");
    }

    /// <inheritdoc/>
    void IChannelConsumer.Consume(IChannelProvider provider)
    {
        if (disposedValue)
            return;

        this.provider = provider;
        connectionIsClosed = false;
        if (initialized)
            ChannelProviderIsAvailable();
    }

    /// <inheritdoc/>
    void IChannelConsumer.ConnectionClosed()
    {
        connectionIsClosed = true;
    }

    /// <inheritdoc/>
    void IChannelConsumer.ConnectionRecovered(bool autorecovered)
    {
        if (currentModel is not null && !autorecovered)
        {
            var old = currentModel;
            currentModel = null;
            _ = GetCurrentModel();
            old.Dispose();
        }

        connectionIsClosed = false;

        if (currentModel is not null)
            ConnectionIsRecovered(autorecovered);
    }

    /// <summary>
    /// <para>
    ///     When the component is created, a channel provider will be required for RabbitMQ.
    ///     This requires an open connection to RabbitMQ.
    /// </para>
    /// <para>
    ///     If you cannot connect to the RabbitMQ server, 
    ///     the channel provider will be assigned later when you are able to connect to the RabbitMQ server.
    /// </para>
    /// <para>
    ///     In this second case, this method will be called.
    /// </para>
    /// </summary>
    protected abstract void ChannelProviderIsAvailable();

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
                consummation.Dispose();
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
}