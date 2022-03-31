namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     This class is used to represent a configured message that is published or received by the application,
///     through a messaging channel/bus.
/// </para>
/// </summary>
public class ChannelDescription
{
    /// <summary>
    /// Creates a new description for the use of messages by the application.
    /// </summary>
    /// <param name="name">See <see cref="Name"/>.</param>
    /// <param name="description">See <see cref="Description"/>.</param>
    /// <param name="messageType">See <see cref="MessageType"/>.</param>
    /// <param name="purpose">See <see cref="Purpose"/>.</param>
    /// <param name="broker">See <see cref="Broker"/>.</param>
    /// <param name="properties">See <see cref="Properties"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///     If any of the required parameters are null.
    /// </exception>
    public ChannelDescription(
        string name,
        string? description,
        Type messageType,
        ChannelPurpose purpose,
        string broker,
        IReadOnlyDictionary<string, string>? properties)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        Purpose = purpose;
        Broker = broker ?? throw new ArgumentNullException(nameof(broker));
        Properties = properties ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// The channel name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Human friendly channel description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Published or received message type.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Determines the purpose for which the message will be used by the application.
    /// </summary>
    public ChannelPurpose Purpose { get; }

    /// <summary>
    /// <para>
    ///     The name of the broker.
    /// </para>
    /// <para>
    ///     Examples:
    ///     <list type="bullet">
    ///         <item>RabbitMQ</item>
    ///         <item>Kafka</item>
    ///     </list>
    /// </para>
    /// </summary>
    public string Broker { get; }

    /// <summary>
    /// <para>
    ///     Properties used for publish or receive messages.
    /// </para>
    /// <para>
    ///     The properties will differ depending on the <see cref="Broker"/>.
    /// </para>
    /// </summary>
    public IReadOnlyDictionary<string, string> Properties { get; }
}
