using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RoyalCode.RabbitMQ.Components.Channels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// Generic component to receive messange from RabbitMQ.
/// </summary>
public class Receiver : BaseComponent
{
    private readonly ChannelInfo channelInfo;
    private readonly ILogger<Publisher> logger;
    private readonly ICollection<MessageListener> listeners = new LinkedList<MessageListener>();
    private readonly Dictionary<MessageListener, EventingBasicConsumer> consumersMap = new();
    private readonly ICollection<MessageListener> waitingListeners = new LinkedList<MessageListener>();

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

    /// <summary>
    /// <para>
    ///     Listens for messages received from RabbitMQ according to the channel settings.
    /// </para>
    /// </summary>
    /// <param name="messageListener">Listener for received messages.</param>
    public void Listen(MessageListener messageListener)
    {
        listeners.Add(messageListener);
        AddConsumer(messageListener);
    }

    private void AddConsumer(MessageListener messageListener)
    {
        lock (consumersMap)
        {
            try
            {
                HandleEventForConsumer(messageListener);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to consume a RabbitMQ queue, channel: {0}", channelInfo);

                if (!waitingListeners.Contains(messageListener))
                    waitingListeners.Add(messageListener);
            }
        }
    }

    private void HandleEventForConsumer(MessageListener messageListener)
    {
        var model = GetChannelToReceive();

        var consumer = new EventingBasicConsumer(model);
        consumer.Received += messageListener.Consumer;

        var ok = channelInfo.GetConsumerQueue(model);
        model.BasicConsume(
            queue: ok.QueueName,
            autoAck: false,
            consumer: consumer);

        consumersMap.Add(messageListener, consumer);
    }

    /// <inheritdoc/>
    protected override void ChannelProviderIsAvailable()
    {
        lock(consumersMap)
        {
            var listeners = waitingListeners.ToArray();
            foreach (var messageListener in listeners)
            {
                HandleEventForConsumer(messageListener);
                waitingListeners.Remove(messageListener);
            }
        }
    }

    /// <inheritdoc/>
    protected override void ConnectionIsRecovered(bool autorecovered)
    {
        if (autorecovered)
        {
            lock (consumersMap)
            {
                foreach (var listener in consumersMap.Keys)
                {
                    waitingListeners.Add(listener);
                }
                consumersMap.Clear();
            }

            ChannelProviderIsAvailable();
        }
    }
}