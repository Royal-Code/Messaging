
namespace RoyalCode.RabbitMQ.Components.Declarations;

/// <summary>
/// Extension methods for <see cref="DeadLetterInfo"/>.
/// </summary>
public static class DeadLetterInfoExtensions
{
    /// <summary>
    /// Enables the use of deadletters.
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo Activate(this DeadLetterInfo deadLetterInfo)
    {
        deadLetterInfo.Active = true;
        return deadLetterInfo;
    }

    /// <summary>
    /// Desativa o uso de deadletters.
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo Deactivate(this DeadLetterInfo deadLetterInfo)
    {
        deadLetterInfo.Active = false;
        return deadLetterInfo;
    }

    /// <summary>
    /// <para>
    ///     Assigns the name of the exchange for deadletters.
    /// </para>
    /// <para>
    ///     It is necessary that the exchange already exists, as it will not be declared.
    /// </para>
    /// <para>
    ///     The exchange already has a default value, 
    ///     which is declared by the constant <see cref="DeadLetterInfo.Constants.DefaultDeadLetterExchange"/>.
    /// </para>
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <param name="exchangeName">The name of the exchange that should be used for deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo UseExchange(this DeadLetterInfo deadLetterInfo, string exchangeName)
    {
        deadLetterInfo.Exchange = exchangeName;
        return deadLetterInfo;
    }

    /// <summary>
    /// <para>
    ///     The queue name will be used for routing deadletters.
    /// </para>
    /// <para>
    ///     This is the default.
    /// </para>
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo UseQueueNameForRouting(this DeadLetterInfo deadLetterInfo)
    {
        deadLetterInfo.RoutingKind = DeadLetterRoutingKind.UseQueueName;
        return deadLetterInfo;
    }

    /// <summary>
    /// <para>
    ///     Assigns the route key that will be used for deadletters.
    /// </para>
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <param name="deadLettersRouteKey">A route key para as deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo UseRouteKey(this DeadLetterInfo deadLetterInfo, string deadLettersRouteKey)
    {
        deadLetterInfo.RoutingKind = DeadLetterRoutingKind.UseSpecifiedValue;
        deadLetterInfo.RoutingKey = deadLettersRouteKey;
        return deadLetterInfo;
    }

    /// <summary>
    /// Configures not to use route key for deadletters.
    /// </summary>
    /// <param name="deadLetterInfo">Object with information for deadletters.</param>
    /// <returns>The same instance of <paramref name="deadLetterInfo"/> for chained calls.</returns>
    public static DeadLetterInfo UseNoneRouteKey(this DeadLetterInfo deadLetterInfo)
    {
        deadLetterInfo.RoutingKind = DeadLetterRoutingKind.None;
        return deadLetterInfo;
    }
}
