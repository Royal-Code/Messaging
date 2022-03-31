namespace RoyalCode.Messaging;

/// <summary>
/// A message to be published by the broker, generated via a <see cref="IPublisher{TMessage}"/>.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
public interface IOutgoingMessage<TMessage> : IOutgoingMessage { }

/// <summary>
/// A message to be published by the broker, generated via a <see cref="IPublisher{TMessage}"/>.
/// </summary>
public interface IOutgoingMessage
{
    /// <summary>
    /// <para>
    ///     The message Id.
    /// </para>
    /// <para>
    ///     If the object type of the sent message has an 'Id' or 'Guid' property of type <see cref="Guid"/>,
    ///     this Id must be the same as the one in the message.
    ///     Otherwise a <see cref="Guid"/> will be generated.
    /// </para>
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Type (class) of message object.
    /// </summary>
    Type MessageType { get; }

    /// <summary>
    /// The body content type (format).
    /// </summary>
    /// <example>
    /// application/json
    /// </example>
    string ContentType { get; }

    /// <summary>
    /// The content (body) encoding.
    /// </summary>
    /// <example>
    /// Utf8
    /// </example>
    string ContentEncoding { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    byte[] Body { get; }

    /// <summary>
    /// <para>
    ///     Collection of properties to send/publish attached to the message.
    /// </para>
    /// <para>
    ///     For example: properties to send in the message header.
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
}