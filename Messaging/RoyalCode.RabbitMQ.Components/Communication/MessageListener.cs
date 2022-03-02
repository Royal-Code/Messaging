using RabbitMQ.Client.Events;
using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Component to listen RabbitMQ messages through a receiver.
/// </para>
/// </summary>
public class MessageListener
{
    /// <summary>
    /// Handler of RabbitMQ events/messages.
    /// </summary>
    public EventHandler<BasicDeliverEventArgs> Consumer { get; }

    /// <summary>
    /// <para>
    ///     Creates a new message listener.
    /// </para>
    /// <para>
    ///     This class is used by <see cref="Receiver"/> to listen for events/messages from RabbitMQ.
    /// </para>
    /// </summary>
    /// <param name="consumer">Handler of the events.</param>
    public MessageListener(EventHandler<BasicDeliverEventArgs> consumer)
    {
        Consumer = consumer;
    }

    /// <summary>
    /// Constructor for inheritance.
    /// </summary>
    protected MessageListener() { }
}