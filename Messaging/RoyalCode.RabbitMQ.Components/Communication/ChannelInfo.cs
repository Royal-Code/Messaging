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
    private QueueDeclareOk? queueDeclareOk;
    private bool tempQueue;

    public ChannelType Type { get; }

    public ExchangeInfo? Exchange { get; }

    public QueueInfo? Queue { get; }

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
        if (queueDeclareOk is null || tempQueue || force)
        {
            queueDeclareOk = Queue?.DeclareQueue(model)
                ?? throw new InvalidOperationException(
                    "This ChannelInfo has no information about a queue and cannot be declared");
        }

        return queueDeclareOk;
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
    public PublicationAddress GetPublicationAddress(IModel model, string? routingKey)
    {
        return Type == ChannelType.Queue 
            ? Queue!.DeclareForPublish(model) 
            : Exchange!.DeclareForPublish(model, routingKey);
    }

    internal void ConnectionRecreated()
    {
        queueDeclareOk = null;
    }
}

public class ExchangeInfo
{
    public ExchangeType Type { get; }

    public QueueInfo? BoundQueue { get; }

    internal PublicationAddress DeclareForPublish(IModel model, string? routingKey)
    {
        throw new NotImplementedException();
    }
}

public class QueueInfo
{
    internal PublicationAddress DeclareForPublish(IModel model)
    {
        throw new NotImplementedException();
    }

    internal QueueDeclareOk DeclareQueue(IModel model)
    {
        throw new NotImplementedException();
    }
}