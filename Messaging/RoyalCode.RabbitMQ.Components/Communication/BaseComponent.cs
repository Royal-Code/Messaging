using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.Communication;

public class BaseComponent : IChannelConsumer
{
    private readonly ChannelStrategy channelStrategy;
    private bool connectionIsClosed = false;
    private IChannelProvider? provider;
    private IModel currentModel;
    

    protected BaseComponent(IChannelManager channelManager, string clusterName, ChannelStrategy channelStrategy)
    {
        channelManager.Consume(clusterName, this);
        this.channelStrategy = channelStrategy;
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
        this.provider = provider;
    }

    /// <inheritdoc/>
    void IChannelConsumer.ConnectionClosed()
    {
        connectionIsClosed = true;
    }

    /// <inheritdoc/>
    void IChannelConsumer.ConnectionRecovered(bool autorecovered)
    {
        throw new NotImplementedException();
    }
}