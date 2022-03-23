namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Information about the connection status.
/// </para>
/// </summary>
public interface IConnectionStatus
{
    /// <summary>
    /// The RabbitMQ Cluster name.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// If is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Count of connection consumers.
    /// </summary>
    int ConsumersCount { get; }
    
    // /// <summary>
    // /// <para>
    // ///     Force a shutdown the current connection.
    // /// </para>
    // /// <para>
    // ///     A reconnection will be made.
    // /// </para>
    // /// </summary>
    // /// <param name="reason">Required reason for shutdown.</param>
    // void Shutdown(string reason);
}