using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using RoyalCode.RabbitMQ.Components.Declarations;

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
    /// <param name="channelManager">A channel channelManager to connection to RabbitMQ and get channels (<see cref="IModel"/>).</param>
    /// <param name="channelStrategy">The strategy for consuming channels.</param>
    /// <param name="channelInfo">Information about the channel where the messages will be published.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is Shared.
    /// </exception>
    public Publisher(
        IChannelManager channelManager,
        ChannelStrategy channelStrategy,
        ChannelInfo channelInfo,
        ILogger<Publisher> logger) 
        : base(channelManager, channelStrategy)
    {
        this.channelInfo = channelInfo;
        this.logger = logger;

        logger.LogDebug("Publish created for channel: {channelInfo}", channelInfo);
    }

    /// <summary>
    /// Check if the channel strategy is valid for a publisher. The shared strategy is not allowed.
    /// </summary>
    /// <param name="channelStrategy">The current strategy.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is Shared.
    /// </exception>
    protected override void GuardChannelStrategy(ChannelStrategy channelStrategy)
    {
        if (channelStrategy == ChannelStrategy.Shared)
            throw new CommunicationException(
                "Shared Channel Strategy is not allowed for publishers. Channels are not thread safe for publication");
    }

    /// <summary>
    /// <para>
    ///     Publish a message to RabbitMQ.
    /// </para>
    /// </summary>
    /// <param name="message">The message details.</param>
    /// <returns>A Task for async processing.</returns>
    /// <exception cref="CommunicationException">
    ///     If the connection to RabbitMQ is closed.
    /// </exception>
    public void Publish(PublicationMessage message)
    {
        if (!Managed.IsOpen)
            throw new CommunicationException(
                "The connection to RabbitMQ is closed, no messages can be published at the moment");

#if NET5_0_OR_GREATER
        var model = Managed.Channel;
#else
        var model = Managed.Channel ?? throw new CommunicationException(
                "The connection to RabbitMQ is closed, no messages can be published at the moment");
#endif
        
        var properties = model.CreateBasicProperties();
        message.ConfigureProperties?.Invoke(properties);

        var address = channelInfo.GetPublicationAddress(model, message.RoutingKey);

        logger.LogDebug("publishing message to: {address}", address);

        model.BasicPublish(address, properties, message.Body);

        logger.LogDebug("message published to: {address}", address);
    }

    /// <summary>
    /// Notify the channel info that the connection has been recreated
    /// when a reconnection occurs and it is not an auto recovery.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="autorecovered">If the connection has been auto recovery.</param>
    protected override void OnReconnected(object? sender, bool autorecovered)
    {
        if (!autorecovered)
            channelInfo.ConnectionRecreated();
    }
}
