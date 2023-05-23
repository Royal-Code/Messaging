using System;
using RoyalCode.RabbitMQ.Components.Declarations;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// Extensions methods for <see cref="QueueInfo"/>.
/// </summary>
public static class QueueInfoExtensions
{
    /// <summary>
    /// Assigns the value of <see cref="QueueInfo.AutoDelete"/>.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="autoDelete">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseAutoDelete(this QueueInfo queue, bool autoDelete)
    {
        queue.AutoDelete = autoDelete;
        return queue;
    }

    /// <summary>
    /// Assigns the value of <see cref="QueueInfo.Durable"/>.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="durable">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseDurable(this QueueInfo queue, bool durable)
    {
        queue.Durable = durable;
        return queue;
    }

    /// <summary>
    /// Assigns the value of <see cref="QueueInfo.Exclusive"/>.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exclusive">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseExclusive(this QueueInfo queue, bool exclusive)
    {
        queue.Exclusive = exclusive;
        return queue;
    }

    /// <summary>
    /// Enables or disables the use of DeadLetter.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="activate">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseDeadLetter(this QueueInfo queue, bool activate = true)
    {
        var deadLetter = queue.DeadLetter;
        deadLetter.Active = activate;
        return queue;
    }

    /// <summary>
    /// Activates DeadLetter and sets the exchange name to DeadLetter.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exchange">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseDeadLetterExchange(this QueueInfo queue, string exchange)
    {
        var deadLetter = queue.DeadLetter;
        deadLetter.Active = true;
        deadLetter.Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        return queue;
    }

    /// <summary>
    /// Activates DeadLetter and sets the routing key to DeadLetter.
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="routingKey">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo UseDeadLetterRoutingKey(this QueueInfo queue, string routingKey)
    {
        var deadLetter = queue.DeadLetter;
        deadLetter.Active = true;
        deadLetter.RoutingKind = DeadLetterRoutingKind.UseSpecifiedValue;
        deadLetter.RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        return queue;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange.
    /// </para>
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exchange">The exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo BindTo(this QueueInfo queue, ExchangeInfo exchange, params string[] routeKeys)
    {
        queue.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(exchange, routeKeys));
        return queue;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type fanout.
    /// </para>
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exchangeName">Nam of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo BindToFanout(this QueueInfo queue, string exchangeName, params string[] routeKeys)
    {
        queue.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Fanout(exchangeName), routeKeys));
        return queue;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type route.
    /// </para>
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exchangeName">Nam of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo BindToRoute(this QueueInfo queue, string exchangeName, params string[] routeKeys)
    {
        queue.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Route(exchangeName), routeKeys));
        return queue;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type topic.
    /// </para>
    /// </summary>
    /// <param name="queue">Queue information.</param>
    /// <param name="exchangeName">Nam of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static QueueInfo BindToTopic(this QueueInfo queue, string exchangeName, params string[] routeKeys)
    {
        queue.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Topic(exchangeName), routeKeys));
        return queue;
    }
}