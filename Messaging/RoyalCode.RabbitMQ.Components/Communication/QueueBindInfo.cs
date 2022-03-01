using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Communication;

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
