namespace RoyalCode.RabbitMQ.Components.Declarations;

/// <summary>
/// <para>
///     The type of the channel exchange.
/// </para>
/// <para>
///     Used by <see cref="ExchangeInfo"/>.
/// </para>
/// </summary>
public enum ExchangeType
{
    /// <summary>
    /// Channel with Exchange fanout type.
    /// </summary>
    Fanout,

    /// <summary>
    /// Channel with Exchange route type.
    /// </summary>
    Route,

    /// <summary>
    /// /// Channel with Exchange topic type.
    /// </summary>
    Topic,
}
