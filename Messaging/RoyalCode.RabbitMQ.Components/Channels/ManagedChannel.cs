using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using System.Collections.Concurrent;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Manage a channel (<see cref="IModel"/>) of the RabbitMQ.
/// </para>
/// </summary>
public abstract class ManagedChannel : IConnectionConsumer
{
    private readonly IConnectionConsumerStatus consumerStatus;
    private readonly ConcurrentBag<IChannelConsumer> consumers = new();
    private readonly ILogger logger;

    private IConnection? connection;
    private IModel? model;

    private bool modelCreated;

    protected ManagedChannel(
        ManagedConnection managedConnection,
        ILogger logger)
    {
        consumerStatus = managedConnection.AddConsumer(this);
        this.logger = logger;
    }

    /// <summary>
    /// Check if the channel is open.
    /// </summary>
    public bool IsOpen => Channel?.IsOpen ?? false;

    /// <summary>
    /// <para>
    ///     Get the channel (<see cref="IModel"/>) of the RabbitMQ.
    /// </para>
    /// <para>
    ///     It can be null if the connection is not established.
    /// </para>
    /// </summary>
    public IModel? Channel => model;

    /// <summary>
    /// <para>
    ///     Consume a channel of RabbitMQ, object of type <see cref="IModel"/>.
    /// </para>
    /// </summary>
    /// <param name="consumer">The channel consumer.</param>
    /// <returns>A <ver cref="IDisposable"/> object to finalize the consumption.</returns>
    IChannelConsumerStatus Consume(IChannelConsumer consumer)
    {
        throw new NotImplementedException();
    }

    void Release()
    {
        throw new NotImplementedException();
    }

    private void CreateModel()
    {
        if (!consumerStatus.IsConnected)
        {
            logger.LogDebug("Connection is not open. Cannot create a channel.");
            RetryCreateChannel();
            return;
        }

        try
        {
            model = connection!.CreateModel();
            ModelCreated(model);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to create a channel.");
            RetryCreateChannel();
        }
    }

    private void RetryCreateChannel()
    {
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            logger.LogInformation("Retrying to create a channel.");
            CreateModel();
        });
    }

    private void ModelCreated(IModel model)
    {
        modelCreated = true;

        logger.LogDebug("Channel created. Notifying consumers.");

        foreach (var consumer in consumers)
        {
            try
            {
                logger.LogDebug("Notifying that the model has been created for the consumer: {consumer}.", consumer);
                consumer.Consume(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed notifying (model created) for the consumer: {consumer).", consumer);
            }
        }
    }

    private void ReCreateModel(bool autorecovered)
    {
        try
        {
            model = connection!.CreateModel();
            ModelReCreated(model, autorecovered);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to re-create a channel.");
            RetryReCreateChannel();
        }
    }

    private void RetryReCreateChannel()
    {
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            logger.LogInformation("Retrying to re-create a channel.");
            CreateModel();
        });
    }

    private void ModelReCreated(IModel model, bool autorecovered)
    {
        logger.LogInformation("Channel re-created. Notifying consumers.");

        foreach (var consumer in consumers)
        {
            try
            {
                logger.LogDebug("Notifying that the model has been re-created for the consumer: {consumer}.", consumer);
                consumer.ConnectionRecovered(model, autorecovered);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed notifying (model re-created) for the consumer: {consumer).", consumer);
            }
        }
    }

    #region IConnectionConsumer implementation


    void IConnectionConsumer.Closed()
    {
        logger.LogInformation("Connection closed. Notifying consumers.");

        connection = null;
        model = null;
        modelCreated = false;

        foreach (var consumer in consumers)
        {
            try
            {
                consumer.ConnectionClosed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed notifying (closed) for the consumer: {consumer).", consumer);
            }
        }
    }

    void IConnectionConsumer.Reloaded(IConnection connection, bool autorecovered)
    {
        if (!modelCreated)
            return;

        this.connection = connection;
        ReCreateModel(autorecovered);
    }

    void IConnectionConsumer.Consume(IConnection connection)
    {
        this.connection = connection;
        CreateModel();
    }

    #endregion
}