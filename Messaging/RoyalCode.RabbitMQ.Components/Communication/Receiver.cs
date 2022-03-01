using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.Communication;

public class Receiver : BaseComponent
{
    private readonly ChannelInfo channelInfo;
    private readonly ILogger<Publisher> logger;

    /// <summary>
    /// Creates a new receiver to listen messages from RabbitMQ.
    /// </summary>
    /// <param name="channelInfo">Information about the channel from where the messages will be received.</param>
    /// <param name="channelManager">A channel manager to connection to RabbitMQ and get channels (<see cref="IModel"/>).</param>
    /// <param name="clusterName">The name of the cluster to connect.</param>
    /// <param name="channelStrategy">The strategy for consuming channels.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is Shared.
    /// </exception>
    public Receiver(
        ChannelInfo channelInfo,
        IChannelManager channelManager,
        string clusterName,
        ChannelStrategy channelStrategy,
        ILogger<Publisher> logger)
        : base(channelManager, clusterName, channelStrategy)
    {
        if (channelStrategy == ChannelStrategy.Pooled)
            throw new CommunicationException(
                "Pooled Channel Strategy is not allowed for receivers. The receivers will not realease the channels.");

        this.channelInfo = channelInfo;
        this.logger = logger;

        logger.LogDebug("Receiver created for channel: {0}", channelInfo);
    }

    public async Task Listen(MessageListener messageListener, CancellationToken cancellationToken)
    {



    }

    protected override void ChannelProviderIsAvailable()
    {
        throw new NotImplementedException();
    }

    protected override void ConnectionIsRecovered(bool autorecovered)
    {
        throw new NotImplementedException();
    }
}

public class MessageListener
{
    
}