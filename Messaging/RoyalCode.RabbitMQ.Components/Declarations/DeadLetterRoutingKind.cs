namespace RoyalCode.RabbitMQ.Components.Declarations;

/// <summary>
/// Routing kind for DeadLetters.
/// </summary>
public enum DeadLetterRoutingKind
{
    /// <summary>
    /// Use the queue name as the route key.
    /// </summary>
    UseQueueName,

    /// <summary>
    /// Uses a specified value (<see cref="DeadLetterInfo.RoutingKey"/>).
    /// </summary>
    UseSpecifiedValue,

    /// <summary>
    /// Route values should not be used.
    /// </summary>
    None,
}