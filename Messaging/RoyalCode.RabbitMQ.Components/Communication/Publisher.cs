using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// A generic component to publish messages to RabbitMQ.
/// </summary>
public class Publisher : BaseComponent
{
    private readonly ChannelInfo channelInfo;
    private readonly ILogger<Publisher> logger;

    /// <summary>
    /// Creates a new publisher to publish messages to RabbitMQ.
    /// </summary>
    /// <param name="channelInfo">Information about the channel where the messages will be published.</param>
    /// <param name="channelManager">A channel manager to connection to RabbitMQ and get channels (<see cref="IModel"/>).</param>
    /// <param name="clusterName">The name of the cluster to connect.</param>
    /// <param name="channelStrategy">The strategy for consuming channels.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is Shared.
    /// </exception>
    public Publisher(
        ChannelInfo channelInfo,
        IChannelManager channelManager,
        string clusterName, 
        ChannelStrategy channelStrategy,
        ILogger<Publisher> logger) 
        : base(channelManager, clusterName, channelStrategy)
    {
        if (channelStrategy == ChannelStrategy.Shared)
            throw new CommunicationException(
                "Shared Channel Strategy is not allowed for publishers. Channels are not thread safe for publication");
        this.channelInfo = channelInfo;
        this.logger = logger;

        logger.LogDebug("Publish created for channel: {0}", channelInfo);
    }

    /// <summary>
    /// <para>
    ///     Publish a message to RabbitMQ.
    /// </para>
    /// </summary>
    /// <param name="message">The message details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Task for async processing.</returns>
    /// <exception cref="CommunicationException">
    ///     If the connection to RabbitMQ is closed.
    /// </exception>
    public async Task Publish(PublicationMessage message, CancellationToken cancellationToken)
    {
        if (ConnectionIsClosed)
            throw new CommunicationException(
                "The connection to RabbitMQ is closed, no messages can be published at the moment");

        var model = await GetChannelToPublishAsync(cancellationToken);

        var properties = model.CreateBasicProperties();
        message.ConfigureProperties?.Invoke(properties);

        var address = channelInfo.GetPublicationAddress(model);

        logger.LogDebug("publishing message to: {0}", address);

        model.BasicPublish(address, properties, message.Body);

        logger.LogDebug("message published to: {0}", address);

        ReturnChannelToPublish(model);
    }

    /// <summary>
    /// Nothing is required to do here.
    /// </summary>
    protected override void ChannelProviderIsAvailable() { }

    /// <summary>
    /// Notify the channel info that the connection has been recreated
    /// when a reconnection occurs and it is not an auto recovery.
    /// </summary>
    /// <param name="autorecovered">If the connection has been auto recovery.</param>
    protected override void ConnectionIsRecovered(bool autorecovered)
    {
        if (!autorecovered)
            channelInfo.ConnectionRecreated();
    }
}
