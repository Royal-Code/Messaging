namespace RoyalCode.Messaging;

/// <summary>
/// Message received by the client from a broker, which can be listened to by a <see cref="IReceiver{TMessage}"/>.
/// </summary>
/// <typeparam name="TMessage">Type of message received.</typeparam>
public interface IIncomingMessage<TMessage> : IIncomingMessage
{
    /// <summary>
    /// The message object.
    /// </summary>
    new TMessage Payload { get; }
}

/// <summary>
/// Message received by the client from a broker, which can be listened to by a <see cref="IReceiver{TMessage}"/>.
/// </summary>
public interface IIncomingMessage
{
    /// <summary>
    /// <para>
    ///     The message Id.
    /// </para>
    /// <para>
    ///     If the object type of the <see cref="Payload"/> has an 'Id' or 'Guid' property of type <see cref="Guid"/>,
    ///     this Id must be the same as the one in the message (<see cref="Payload"/>).
    /// </para>
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Type (class) of message object.
    /// </summary>
    Type MessageType { get; }

    /// <summary>
    /// <para>
    ///     Collection of sent/published properties attached to the message.
    /// </para>
    /// <para>
    ///     For example: properties sent in the message header.
    /// </para>
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Properties { get; }

    /// <summary>
    /// <para>
    ///     User sender of the message (token, code, subject, identifier).
    /// </para>
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// The broker name (ex.: RabbitMQ, Kafka).
    /// </summary>
    string Broker { get; }

    /// <summary>
    /// The message object.
    /// </summary>
    object Payload { get; }

    /// <summary>
    /// <para>
    ///     Flags the message to be rejected.
    /// </para>
    /// <para>
    ///     The message will be discarded, and may fall into a deadletter queue, if configured.
    /// </para>
    /// </summary>
    void Reject();

    /// <summary>
    /// <para>
    ///     Flags the message as rejected but to be redelivered.
    /// </para>
    /// <para>
    ///     By default the sleep time is 2000 milliseconds. 
    ///     This value can be changed via the <paramref name="sleepTime"/> parameter.
    /// </para>
    /// <para>
    ///     Sleep time is important for redelivery, 
    ///     because the system can be in a loop asking for redeliveries
    ///     and this can end up consuming unnecessary resources.
    /// </para>
    /// </summary>
    /// <param name="sleepTime">Sleep time in milliseconds.</param>
    void Redelivery(int sleepTime = 2000);
}
