using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Communication;

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
