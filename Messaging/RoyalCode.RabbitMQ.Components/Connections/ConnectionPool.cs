using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Connections;

internal class ConnectionPool : IConnectionPool
{
    private ConnectionFactory[] connectionFactories;
    private int nextConnectionIndex = 0;

    public ConnectionPool(
        bool shouldTryBackToFirstConnection,
        TimeSpan retryConnectionDelay,
        ConnectionFactory[] connectionFactories)
    {
        ShouldTryBackToFirstConnection = shouldTryBackToFirstConnection;
        RetryConnectionDelay = retryConnectionDelay;
        this.connectionFactories = connectionFactories;
    }

    /// <summary>
    /// If connection manager should try to go back to the first connection.
    /// </summary>
    public bool ShouldTryBackToFirstConnection { get; }

    /// <summary>
    /// Waiting time to try to connect again.
    /// </summary>
    public TimeSpan RetryConnectionDelay { get; }

    /// <summary>
    /// Get the cluster next connection.
    /// </summary>
    /// <returns>New instance of <see cref="IConnection"/>.</returns>
    public IConnection GetNextConnetion()
    {
        var cf = connectionFactories[nextConnectionIndex];
        AddNextConnectionIndex();
        return cf.CreateConnection();
    }

    private void AddNextConnectionIndex()
    {
        nextConnectionIndex++;
        if (nextConnectionIndex >= connectionFactories.Length)
            nextConnectionIndex = 0;
    }
}