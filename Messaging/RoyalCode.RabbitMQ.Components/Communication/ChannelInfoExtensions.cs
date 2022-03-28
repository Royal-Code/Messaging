namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Extensions methods for <see cref="ChannelInfo"/>.
/// </para>
/// </summary>
public static class ChannelInfoExtensions
{
    /// <summary>
    /// Assigns the value of <see cref="ExchangeInfo.AutoDelete"/> or <see cref="QueueInfo.AutoDelete"/>.
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="autoDelete">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo UseAutoDelete(this ChannelInfo channel, bool autoDelete = true)
    {
        if (channel.Type is ChannelType.Exchange)
            channel.Exchange!.AutoDelete = autoDelete;
        else
            channel.Queue!.AutoDelete = autoDelete;

        return channel;
    }

    /// <summary>
    /// Assigns the value of <see cref="ExchangeInfo.Durable"/> or <see cref="QueueInfo.Durable"/>.
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="durable">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo UseDurable(this ChannelInfo channel, bool durable = true)
    {
        if (channel.Type is ChannelType.Exchange)
            channel.Exchange!.Durable = durable;
        else
            channel.Queue!.Durable = durable;

        return channel;
    }

    /// <summary>
    /// Assigns the value of <see cref="QueueInfo.Exclusive"/>.
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exclusive">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo UseExclusive(this ChannelInfo channel, bool exclusive)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("The Exclusive property can only be configurated for queues, and this channel type is exchange.");

        channel.Queue!.Exclusive = exclusive;
        return channel;
    }

    /// <summary>
    /// Enables or disables the use of DeadLetter.
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="activate">Value for the property.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo UseDeadLetter(this ChannelInfo channel, bool activate = true)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Deadletter can only be configurated for queues, and this channel type is exchange.");

        var deadLetter = channel.Queue!.DeadLetter;
        deadLetter.Active = activate;

        return channel;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exchange">The exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo BindTo(this ChannelInfo channel, ExchangeInfo exchange, params string[] routeKeys)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Bind To can only be configurated for queues, and this channel type is exchange.");

        channel.Queue!.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(exchange, routeKeys));
        return channel;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exchangeChannel">A exchange channel to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo BindTo(this ChannelInfo channel, ChannelInfo exchangeChannel, params string[] routeKeys)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Bind To can only be configurated for queues, and this channel type is exchange.");

        if (exchangeChannel.Type is not ChannelType.Exchange)
            throw new ChannelConfigurationException("The exchange channel must be of exchange type, and type is queue.");

        channel.Queue!.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(exchangeChannel.Exchange!, routeKeys));
        return channel;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type fanout.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exchangeName">Name of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo BindToFanout(this ChannelInfo channel, string exchangeName, params string[] routeKeys)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Bind To can only be configurated for queues, and this channel type is exchange.");

        channel.Queue!.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Fanout(exchangeName), routeKeys));
        return channel;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type route.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exchangeName">Name of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo BindToRoute(this ChannelInfo channel, string exchangeName, params string[] routeKeys)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Bind To can only be configurated for queues, and this channel type is exchange.");

        channel.Queue!.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Route(exchangeName), routeKeys));
        return channel;
    }

    /// <summary>
    /// <para>
    ///     Bind the queue to an exchange of type topic.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="exchangeName">Name of the exchange to bind.</param>
    /// <param name="routeKeys">Routing keys.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo BindToTopic(this ChannelInfo channel, string exchangeName, params string[] routeKeys)
    {
        if (channel.Type is not ChannelType.Queue)
            throw new ChannelConfigurationException("Bind To can only be configurated for queues, and this channel type is exchange.");

        channel.Queue!.BindInfo.BoundExchangeInfos.Add(new BoundExchangeInfo(ExchangeInfo.Topic(exchangeName), routeKeys));
        return channel;
    }

    /// <summary>
    /// <para>
    ///     Adds a property for exchange declaration.
    /// </para>
    /// </summary>
    /// <param name="channel">The channel information.</param>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <returns>The same instance for chained calls.</returns>
    public static ChannelInfo AddProperty(this ChannelInfo channel, string name, object value)
    {
        if (channel.Type is not ChannelType.Exchange)
            throw new ChannelConfigurationException("AddProperty can only be configurated for exchanges, and this channel type is queue.");

        channel.Exchange!.Properties[name] = value;
        return channel;
    }
}
