using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Communication;

namespace RoyalCode.RabbitMQ.Components.Declarations;

/// <summary>
/// <para>
///     Contains information about a exchange.
/// </para>
/// <para>
///     This information will be used to declare the exchange, and define publishing addresses.
/// </para>
/// </summary>
public class ExchangeInfo
{
    /// <summary>
    /// Creates a new info for an exchange of fanout type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <returns>New instance of <see cref="ExchangeInfo"/>.</returns>
    public static ExchangeInfo Fanout(string name) => new(name, ExchangeType.Fanout);

    /// <summary>
    /// Creates a new info for an exchange of route type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="routingKey">Optional, default routing key.</param>
    /// <returns>New instance of <see cref="ExchangeInfo"/>.</returns>
    public static ExchangeInfo Route(string name, string? routingKey = null) => new(name, ExchangeType.Route, routingKey);

    /// <summary>
    /// Creates a new info for an exchange of topic type.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="routingKey">Optional, default routing key.</param>
    /// <returns>New instance of <see cref="ExchangeInfo"/>.</returns>
    public static ExchangeInfo Topic(string name, string? routingKey = null) => new(name, ExchangeType.Topic, routingKey);

    private readonly Dictionary<string, PublicationAddress> publicationAddresses = new();
    private bool isDeclared;

    /// <summary>
    /// Type of the Exchange.
    /// </summary>
    public ExchangeType Type { get; }

    /// <summary>
    /// Name of the Exchange.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Default routing key, used in publication address creation when none routing key is informed.
    /// </summary>
    public string? DefaultRoutingKey { get; }

    /// <summary>
    /// Configuration of the exchange, whether it is durable.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Configuration of the exchange, whether it is deleted after disconnection.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Properties used to declare the exchange.
    /// </summary>
    public Dictionary<string, object> Properties { get; } = new();

    /// <summary>
    /// Creates a new persistent Exchange.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="type">Type of the Exchange.</param>
    /// <param name="defaultRoutingKey">
    ///     Optional, default routing key, used in publication address creation when none routing key is informed.
    /// </param>
    public ExchangeInfo(string name, ExchangeType type, string? defaultRoutingKey = null)
    {
        Name = name;
        Type = type;
        DefaultRoutingKey = defaultRoutingKey;
    }

    /// <summary>
    /// Declare the exchange, if not declared yet.
    /// </summary>
    /// <param name="model">The RabbitMQ <see cref="IModel"/>.</param>
    /// <param name="force">Force declaration, tf redeclaration is required.</param>
    internal void Declare(IModel model, bool force = false)
    {
        if (!isDeclared || force)
        {
            model.ExchangeDeclare(
                exchange: Name,
                type: Type.ToExchageType(),
                durable: Durable,
                autoDelete: AutoDelete,
                arguments: Properties);

            isDeclared = true;
        }
    }

    /// <summary>
    /// Resets the declarations, makes the declarations can occur again. 
    /// This is useful when there are reconnections.
    /// </summary>
    public void ResetDeclarations()
    {
        isDeclared = false;
    }

    internal PublicationAddress GetPublicationAddress(IModel model, string? routingKey = null)
    {
        if (!isDeclared)
            Declare(model);

        routingKey ??= (DefaultRoutingKey ?? string.Empty);

        if (publicationAddresses.ContainsKey(routingKey))
            return publicationAddresses[routingKey];

        var publicationAddress = new PublicationAddress(string.Empty, Name, routingKey);
        publicationAddresses[routingKey] = publicationAddress;
        return publicationAddress;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type.ToExchageType()}://{Name}";
    }
}
