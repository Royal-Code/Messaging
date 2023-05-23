using RoyalCode.RabbitMQ.Components.Declarations;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// Extension methods for <see cref="ExchangeInfo"/>.
/// </summary>
public static class ExchangeInfoExtensions
{
    /// <summary>
    /// Assigns the value of <see cref="ExchangeInfo.AutoDelete"/>.
    /// </summary>
    /// <param name="queue">Exchange information.</param>
    /// <param name="autoDelete">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ExchangeInfo UseAutoDelete(this ExchangeInfo queue, bool autoDelete)
    {
        queue.AutoDelete = autoDelete;
        return queue;
    }

    /// <summary>
    /// Assigns the value of <see cref="ExchangeInfo.Durable"/>.
    /// </summary>
    /// <param name="exchange">Exchange information.</param>
    /// <param name="durable">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ExchangeInfo UseDurable(this ExchangeInfo exchange, bool durable)
    {
        exchange.Durable = durable;
        return exchange;
    }

    /// <summary>
    /// <para>
    ///     Adds a property for exchange declaration.
    /// </para>
    /// </summary>
    /// <param name="exchange">Exchange information.</param>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ExchangeInfo AddProperty(this ExchangeInfo exchange, string name, object value)
    {
        exchange.Properties[name] = value;
        return exchange;
    }
}
