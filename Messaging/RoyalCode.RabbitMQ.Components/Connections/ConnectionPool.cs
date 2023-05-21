using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Connections;

internal sealed class ConnectionPool : IConnectionPool
{
    private readonly ConnectionFactory[] connectionFactories;
    private readonly ILogger logger;
    private volatile IConnection? currentConnection;
    private int nextConnectionIndex;

    private bool disposing;

    public ConnectionPool(
        bool shouldTryBackToFirstConnection,
        TimeSpan retryConnectionDelay,
        ConnectionFactory[] connectionFactories,
        ILogger logger)
    {
        ShouldTryBackToFirstConnection = shouldTryBackToFirstConnection;
        RetryConnectionDelay = retryConnectionDelay;
        this.connectionFactories = connectionFactories;
        this.logger = logger;
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
        logger.LogInformation("Creating a connection to RabbitMQ");

        var cf = connectionFactories[nextConnectionIndex];
        AddNextConnectionIndex();

        var newConnection = cf.CreateConnection();
        currentConnection?.Dispose();
        currentConnection = newConnection;

        return currentConnection;
    }

    public void TryReconnect(Action<IConnection, bool> callback)
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var thread = new Thread(Reconnector);
        thread.Start(callback);
    }

    private void AddNextConnectionIndex()
    {
        nextConnectionIndex++;
        if (nextConnectionIndex >= connectionFactories.Length)
            nextConnectionIndex = 0;
    }

    private void Reconnector(object? callbackObject)
    {
        var callback = (Action<IConnection, bool>)callbackObject!;

        if (currentConnection?.IsOpen ?? false)
            callback(currentConnection, true);

        int attempts = 0;
        while(true)
        {
            Thread.Sleep(RetryConnectionDelay);

            if (disposing)
                break;

            if (currentConnection?.IsOpen ?? false)
            {
                callback(currentConnection, true);
                break;
            }

            try
            {
                attempts++;

                var connection = GetNextConnetion();
                logger.LogInformation("Reconnection to RabbitMQ has been successfully completed");

                callback(connection, false);

                if (ShouldTryBackToFirstConnection && nextConnectionIndex is not 1)
                {
                    var thread = new Thread(BackToFirstConnection);
                    thread.Start(callback);
                }

                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to reconnect to RabbitMQ. Attempts: {Attempts}. A new attempt will be done in {TotalSeconds}s",
                    attempts,
                    RetryConnectionDelay.TotalSeconds);
            }
        }

        if (disposing && currentConnection is not null)
        {
            currentConnection?.Dispose();
            currentConnection = null;
        }
    }

    private void BackToFirstConnection(object? callbackObject)
    {
        if (callbackObject is null) 
            throw new ArgumentNullException(nameof(callbackObject));


        var callback = (Action<IConnection, bool>)callbackObject;

        logger.LogInformation(
            "The current connection with RabbitMQ node is not the first, attempts to back to the first node will be made");

        int attempts = 0;
        while (true)
        {
            Thread.Sleep(RetryConnectionDelay);
            
            if (disposing)
                break;

            try
            {
                attempts++;

                var cf = connectionFactories[0];
                var connection = cf.CreateConnection();

                callback(connection, false);

                currentConnection?.Dispose();
                currentConnection = connection;
                nextConnectionIndex = 1;

                logger.LogInformation("Reconnection to the first node of RabbitMQ has been successfully completed");

                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to reconnect to the first node of RabbitMQ. Attempts: {Attempts}. A new attempt will be done in {TotalSeconds}s",
                    attempts,
                    RetryConnectionDelay.TotalSeconds);
            }
        }

        if (disposing && currentConnection is not null)
        {
            currentConnection?.Dispose();
            currentConnection = null;
        }
    }

    public void Dispose()
    {
        if (disposing)
            return;

        disposing = true;

        logger.LogDebug("Disposing the connection pool");

        currentConnection = null;
    }
}