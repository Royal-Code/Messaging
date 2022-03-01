using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

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

    /// <inheritdoc/>
    public override string ToString()
    {
        return RoutingKeys.Length > 0
            ? $"{BoundExchange.Name}={string.Join("|", RoutingKeys)}"
            : BoundExchange.Name;
    }
}