using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RoyalCode.RabbitMQ.Components.Channels;
using RoyalCode.RabbitMQ.Components.Declarations;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// Generic component to receive messange from RabbitMQ.
/// </summary>
public class Receiver : BaseChannelConsumer
{
    private readonly ChannelInfo channelInfo;
    private readonly ILogger logger;
    private readonly ICollection<MessageListener> listeners = new LinkedList<MessageListener>();
    private readonly Dictionary<MessageListener, EventingBasicConsumer> consumersMap = new();
    private readonly ICollection<MessageListener> waitingListeners = new LinkedList<MessageListener>();

    /// <summary>
    /// Creates a new receiver to listen messages from RabbitMQ.
    /// </summary>
    /// <param name="channelManager">A channel manager to connection to RabbitMQ and get channels (<see cref="IModel"/>).</param>
    /// <param name="channelStrategy">The strategy for consuming channels.</param>
    /// <param name="channelInfo">Information about the channel from where the messages will be received.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="CommunicationException">
    ///     If the stratery is Shared.
    /// </exception>
    public Receiver(
        IChannelManager channelManager,
        ChannelStrategy channelStrategy,
        ChannelInfo channelInfo,
        ILogger<Receiver> logger)
        : base(channelManager, channelStrategy)
    {
        this.channelInfo = channelInfo;
        this.logger = logger;

        logger.LogDebug("Receiver created for channel: {channelInfo}", channelInfo);
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
                if (!Managed.IsOpen)
                {
                    AddToWaitingListeners(messageListener);
                    return;
                }

                HandleEventForConsumer(messageListener);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to consume a RabbitMQ queue, channel: {channelInfo}", channelInfo);

                AddToWaitingListeners(messageListener);
            }
        }
    }

    private void AddToWaitingListeners(MessageListener messageListener)
    {
        if (!waitingListeners.Contains(messageListener))
            waitingListeners.Add(messageListener);
    }

    private void HandleEventForConsumer(MessageListener messageListener)
    {
        var model = Managed.Channel ?? throw new CommunicationException("Channel is closed");

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
    public override void Consume(IModel channel)
    {
        lock (consumersMap)
        {
            var currentWaitingListeners = waitingListeners.ToArray();
            foreach (var messageListener in currentWaitingListeners)
            {
                try
                { 
                    HandleEventForConsumer(messageListener);
                    waitingListeners.Remove(messageListener);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to consume a RabbitMQ queue, channel: {channelInfo}", channelInfo);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void Reloaded(IModel channel, bool autorecovered)
    {
        if (!autorecovered)
        {
            lock (consumersMap)
            {
                foreach (var listener in consumersMap.Keys)
                {
                    waitingListeners.Add(listener);
                }
                consumersMap.Clear();
            }

            Consume(channel);
        }
    }
}