using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Communication;

namespace RoyalCode.RabbitMQ.Components.Declarations;

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
    private Dictionary<string, object>? arguments;

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
    public bool AutoDelete { get; set; }

    /// <summary>
    /// Configuration of the queue, if it is exclusive.
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// <para>
    ///     Information for deadletter use.
    /// </para>
    /// <para>
    ///     By default, it is not active.
    /// </para>
    /// </summary>
    public DeadLetterInfo DeadLetter { get; } = new();

    /// <summary>
    /// <para>
    ///     Information to bind the queue to one or more exchanges.
    /// </para>
    /// </summary>
    public QueueBindInfo BindInfo { get; } = new();

    /// <summary>
    /// <para>
    ///     Add an argument to the queue.
    /// </para>
    /// <para>
    ///     The arguments will be used in the queue declaration.
    /// </para>
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The argument value.</param>
    /// <exception cref="ArgumentException">If the name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">If value is null.</exception>
    public void AddArgument(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        arguments ??= new();
        arguments[name] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// <para>
    ///     Create arguments for queue declaration.
    /// </para>
    /// </summary>
    /// <returns>A dictionary instance with the arguments.</returns>
    public Dictionary<string, object> CreateArguments()
    {
        var declarationArguments = DeadLetter.CreateArguments(Name);

        if (arguments is null)
            return declarationArguments;

        foreach (var kvp in arguments)
            declarationArguments[kvp.Key] = kvp.Value;

        return declarationArguments;
    }

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
            queueDeclareOk = model.QueueDeclare(
                queue: Name,
                durable: Durable,
                exclusive: Exclusive,
                autoDelete: AutoDelete,
                arguments: CreateArguments());
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

    /// <summary>
    /// Informações sobre a queue.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var bindings = BindInfo.ToString();

        return bindings.Length > 0
            ? $"Queue://{Name}?{bindings}"
            : $"Queue://{Name}";
    }
}
