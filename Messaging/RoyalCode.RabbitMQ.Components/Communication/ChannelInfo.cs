using RabbitMQ.Client;
using System;
using System.Collections.Generic;

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
    public PublicationAddress GetPublicationAddress(IModel model, string? routingKey)
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
}

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
    public Dictionary<string, object> Properties = new Dictionary<string, object>();

    /// <summary>
    /// Creates a new persistent Exchange.
    /// </summary>
    /// <param name="name">Name of the Exchange.</param>
    /// <param name="type">Type of the Exchange.</param>
    public ExchangeInfo(string name, ExchangeType type)
    {
        Name = name;
        Type = type;
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

        routingKey ??= string.Empty;

        if (publicationAddresses.ContainsKey(routingKey))
            return publicationAddresses[routingKey];

        var publicationAddress = new PublicationAddress(string.Empty, Name, routingKey);
        publicationAddresses[routingKey] = publicationAddress;
        return publicationAddress;
    }
}

/// <summary>
/// <para>
///     Contains information about a queue.
/// </para>
/// <para>
///     This information will be used to declare the queue, and define publishing addresses.
/// </para>
/// </summary>
public class QueueInfo
{
    /// <summary>
    /// <para>
    ///     Creates a new durable, persistent, not exclusive Queue.
    /// </para>
    /// </summary>
    /// <param name="name">Name of the Queue.</param>
    /// <returns>A new instance of <see cref="QueueInfo"/>.</returns>
    public static QueueInfo Persistent(string name) => new(name);

    /// <summary>
    /// <para>
    ///     Creates a new durable, persistent, not exclusive Queue with dead letters activated.
    /// </para>
    /// </summary>
    /// <param name="name">Name of the Queue.</param>
    /// <returns>A new instance of <see cref="QueueInfo"/>.</returns>
    public static QueueInfo PersistentWithDeadLetter(string name) => new QueueInfo(name).UseDeadLetter();

    /// <summary>
    /// <para>
    ///     Creates a new exclusive and temporary queue.
    /// </para>
    /// <para>
    ///     The queue will be deleted after disconnection.
    /// </para>
    /// </summary>
    /// <param name="name">Optional queue name.</param>
    /// <returns>A new instance of <see cref="QueueInfo"/>.</returns>
    public static QueueInfo TemporaryAndExclusive(string? name = null) => new(true, name);

    /// <summary>
    /// <para>
    ///     Creates a new exclusive and temporary queue and bind to the exchange.
    /// </para>
    /// <para>
    ///     The queue will be deleted after disconnection.
    /// </para>
    /// </summary>
    /// <param name="exchange">The exchange.</param>
    /// <param name="name">Optional queue name.</param>
    /// <returns>A new instance of <see cref="QueueInfo"/>.</returns>
    public static QueueInfo TemporaryBoundTo(ExchangeInfo exchange, string? name = null)
        => new QueueInfo(true, name).BindTo(exchange);

    private QueueDeclareOk? queueDeclareOk;
    private PublicationAddress? publicationAddress;

    /// <summary>
    /// Create a new Queue information with default options (durable, persistent, not exclusive).
    /// </summary>
    /// <param name="name">Name of the Queue.</param>
    public QueueInfo(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Internal for create temporary queues.
    /// </summary>
    /// <param name="temporary">If is temporary.</param>
    /// <param name="name">Optional queue name.</param>
    private QueueInfo(bool temporary, string? name) : this(name ?? string.Empty)
    {
        Temporary = temporary;
        Durable = false;
        AutoDelete = true;
        Exclusive = true;
    }

    /// <summary>
    /// Name of the Queue.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Determines whether the queue is temporary.
    /// </summary>
    public bool Temporary { get; }

    /// <summary>
    /// Configuration of the queue, whether it is durable.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Configuration of the queue, whether it is deleted after disconnection.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Configuration of the queue, if it is exclusive.
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// <para>
    ///     Information for deadletter use.
    /// </para>
    /// <para>
    ///     By default, it is not active.
    /// </para>
    /// </summary>
    public DeadLetterInfo DeadLetter { get; } = new DeadLetterInfo();

    /// <summary>
    /// <para>
    ///     Information to bind the queue to one or more exchanges.
    /// </para>
    /// </summary>
    public QueueBindInfo BindInfo { get; } = new QueueBindInfo();

    internal QueueDeclareOk GetQueueDeclaration(IModel model, bool force = false)
    {
        if (!force && queueDeclareOk is not null)
            return queueDeclareOk;

        if (Temporary)
        {
            queueDeclareOk = model.QueueDeclare(Name);
        }
        else
        {
            var arguments = DeadLetter.CreateArguments(Name);

            queueDeclareOk = model.QueueDeclare(
                queue: Name,
                durable: Durable,
                exclusive: Exclusive,
                autoDelete: AutoDelete,
                arguments: arguments);
        }

        foreach (var bind in BindInfo.BoundExchangeInfos)
        {
            bind.BoundExchange.Declare(model, force);
            foreach (var route in bind.GetRouteKeys())
            {
                model.QueueBind(
                    queue: queueDeclareOk.QueueName,
                    exchange: bind.BoundExchange.Name,
                    routingKey: route);
            }
        }

        return queueDeclareOk;
    }

    internal PublicationAddress GetPublicationAddress(IModel model)
    {
        return publicationAddress 
            ??= new PublicationAddress(string.Empty, string.Empty, GetQueueDeclaration(model).QueueName);
    }

    internal void ResetDeclarations()
    {
        queueDeclareOk = null;
        publicationAddress = null;
    }
}

/// <summary>
/// <para>
///     Information about the bounds between the queue and one or more exchanges.
/// </para>
/// </summary>
public class QueueBindInfo
{
    /// <summary>
    /// <para>
    ///     Collection of bounds between the queue and the exchanges.
    /// </para>
    /// <para>
    ///     There is usually only one bound, when there is one.
    /// </para>
    /// </summary>
    public ICollection<BoundExchangeInfo> BoundExchangeInfos { get; } = new LinkedList<BoundExchangeInfo>();
}

/// <summary>
/// <para>
///     Information of a binding between a queue and an exchange.
/// </para>
/// </summary>
public class BoundExchangeInfo
{
    /// <summary>
    /// Creates a new bound info with the exchange info.
    /// </summary>
    /// <param name="boundExchange">Exchange bound to the queue.</param>
    /// <param name="routingKeys"></param>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref name="boundExchange"/> is null.
    /// </exception>
    public BoundExchangeInfo(ExchangeInfo boundExchange, params string[] routingKeys)
    {
        BoundExchange = boundExchange ?? throw new ArgumentNullException(nameof(boundExchange));
        RoutingKeys = routingKeys;
    }

    /// <summary>
    /// <para>
    ///     Exchange bound to the queue.
    /// </para>
    /// <para>
    ///     If a queue is bound to the exchange, this value must be supplied.
    /// </para>
    /// </summary>
    public ExchangeInfo BoundExchange { get; }

    /// <summary>
    /// The routings keys to bind.
    /// </summary>
    public string[] RoutingKeys { get; set; }

    internal string[] GetRouteKeys() => RoutingKeys.Length > 0
        ? RoutingKeys
        : new string[] { string.Empty };
}