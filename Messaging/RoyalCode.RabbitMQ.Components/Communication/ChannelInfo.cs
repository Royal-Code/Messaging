using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Component for determining information of the RabbitMQ channel used for publishing or receiving messages.
/// </para>
/// <para>
///     This component describes how messages will be published or received.
/// </para>
/// <para>
///     In the case of publishing, 
///     it describes whether it will be in queues or exchanges and defines the queue or exchange information.
/// </para>
/// <para>
///     In cases of receiving messages, 
///     it describes which queue will be listened to and if there are links to exchanges.
/// </para>
/// <para>
///     This component also applies the declaration of queues and exchanges.
/// </para>
/// </summary>
public class ChannelInfo
{
    /// <summary>
    /// <para>
    ///     Creates a new <see cref="ChannelInfo"/> for a durable, persistent, not exclusive Queue.
    /// </para>
    /// </summary>
    /// <param name="name">Name of the Queue.</param>
    /// <returns>A new instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo PersistentQueue(string name) => new(QueueInfo.Persistent(name));

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="ChannelInfo"/> for a durable, persistent, not exclusive Queue with dead letters activated.
    /// </para>
    /// </summary>
    /// <param name="name">Name of the Queue.</param>
    /// <returns>A new instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo QueueWithDeadLetter(string name)
        => new(QueueInfo.PersistentWithDeadLetter(name));

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="ChannelInfo"/> for a exclusive and temporary queue.
    /// </para>
    /// </summary>
    /// <param name="name">Optional queue name.</param>
    /// <returns>A new instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo TemporaryQueue(string? name = null) => new(QueueInfo.TemporaryAndExclusive(name));

    /// <summary>
    /// Creates a new <see cref="ChannelInfo"/> for an exchange of fanout type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <returns>New instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo FanoutExchange(string name) => new(ExchangeInfo.Fanout(name));

    /// <summary>
    /// Creates a new <see cref="ChannelInfo"/> for an exchange of route type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="routingKey">Optional, default routing key.</param>
    /// <returns>New instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo RouteExchange(string name, string? routingKey = null)
        => new(ExchangeInfo.Route(name, routingKey));

    /// <summary>
    /// Creates a new <see cref="ChannelInfo"/> for an exchange of topic type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="routingKey">Optional, default routing key.</param>
    /// <returns>New instance of <see cref="ChannelInfo"/>.</returns>
    public static ChannelInfo TopicExchange(string name, string? routingKey = null)
        => new(ExchangeInfo.Topic(name, routingKey));

    /// <summary>
    /// <para>
    ///     The channel type, configuration to publish to a exchange or to a queue.
    /// </para>
    /// <para>
    ///     When used for receive messages, it is always a queue.
    /// </para>
    /// </summary>
    public ChannelType Type { get; }

    /// <summary>
    /// The exchange information.
    /// </summary>
    public ExchangeInfo? Exchange { get; }

    /// <summary>
    /// The queue information.
    /// </summary>
    public QueueInfo? Queue { get; }

    /// <summary>
    /// <para>
    ///     Creates a new information about a channel with RabbitMQ that can publish messages to an exchange.
    /// </para>
    /// </summary>
    /// <param name="exchange">Information about the exchange.</param>
    /// <exception cref="ArgumentNullException">
    /// <para>
    ///     When <paramref name="exchange"/> is null.
    /// </para>
    /// </exception>
    public ChannelInfo(ExchangeInfo exchange)
    {
        Type = ChannelType.Exchange;
        Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
    }

    /// <summary>
    /// <para>
    ///     Creates a new information about a channel with RabbitMQ that can consume ou publish messages to a queue.
    /// </para>
    /// </summary>
    /// <param name="queue">Information about the queue.</param>
    /// <para>
    ///     When <paramref name="queue"/> is null.
    /// </para>
    public ChannelInfo(QueueInfo queue)
    {
        Type = ChannelType.Queue;
        Queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    /// <summary>
    /// <para>
    ///     Declares a Queue to be used by consumers.
    /// </para>
    /// <para>
    ///     The queue is declared only once, always returning the same <see cref="QueueDeclareOk"/> object on subsequent calls.
    /// </para>
    /// <para>
    ///     To force the declaration use <c>true</c> for the <paramref name="force"/> parameter.
    /// </para>
    /// <para>
    ///     When there is a reconnection, the queue will be declared again.
    /// </para>
    /// </summary>
    /// <param name="model">Rabbit's <see cref="IModel"/> for declaration.</param>
    /// <param name="force">Force the queue declaration.</param>
    /// <returns><see cref="QueueDeclareOk"/>.</returns>
    public QueueDeclareOk GetConsumerQueue(IModel model, bool force = false)
    {
        return Queue?.GetQueueDeclaration(model, force)
            ?? throw new InvalidOperationException(
                "This ChannelInfo has no information about a queue and cannot be declared");
    }

    /// <summary>
    /// <para>
    ///     Declares an Exchange or Queue for sending messages.
    /// </para>
    /// <para>
    ///     Returns the address to publish the message.
    /// </para>
    /// </summary>
    /// <param name="model">Rabbit's <see cref="IModel"/> for declaration.</param>
    /// <param name="routingKey">Optional rounting key for publication on exchanges.</param>
    /// <returns>The publication address.</returns>
    public PublicationAddress GetPublicationAddress(IModel model, string? routingKey = null)
    {
        return Type == ChannelType.Queue
            ? Queue!.GetPublicationAddress(model)
            : Exchange!.GetPublicationAddress(model, routingKey);
    }

    internal void ConnectionRecreated()
    {
        Queue?.ResetDeclarations();
        Exchange?.ResetDeclarations();
    }

    /// <summary>
    /// Generates a string with the channel properties.
    /// </summary>
    /// <returns><see cref="string"/>.</returns>
    public override string ToString()
    {
        return Type is ChannelType.Exchange
            ? Exchange!.ToString()
            : Queue!.ToString();
    }
}
