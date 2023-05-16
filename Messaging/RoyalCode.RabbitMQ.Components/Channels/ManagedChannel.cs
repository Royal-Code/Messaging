using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Manage a channel (<see cref="IModel"/>) of the RabbitMQ.
/// </para>
/// </summary>
public abstract class ManagedChannel : IConnectionConsumer
{
    private readonly IConnectionConsumerStatus consumerStatus;
    private readonly ConcurrentQueue<IChannelConsumer> consumers = new();
    private readonly ILogger logger;
    private readonly EventHandler<ShutdownEventArgs> onModelShutdowEventHander;

    private IConnection? connection;
    private IModel? model;

    private bool modelCreated;

    /// <summary>
    /// Initialize a new instance of <see cref="ManagedChannel"/>.
    /// </summary>
    /// <param name="managedConnection">The managed connection.</param>
    /// <param name="logger">The logger.</param>
    protected ManagedChannel(
        ManagedConnection managedConnection,
        ILogger logger)
    {
        consumerStatus = managedConnection.AddConsumer(this);
        this.logger = logger;
        onModelShutdowEventHander = OnModelShutdown;
    }

    /// <summary>
    /// Check if the channel is open.
    /// </summary>
#if NET5_0_OR_GREATER
    [MemberNotNullWhen(true, nameof(Channel), nameof(model))]
#endif
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
    public IChannelConsumerStatus Consume(IChannelConsumer consumer)
    {
        consumers.Enqueue(consumer);

        if (IsOpen)
        {
            try
            {
                consumer.Consume(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to consume the channel.");
            }
        }

        return new ManagedConsumer(this, consumer);
    }

    internal void Release(IChannelConsumer consumer)
    {
        // todo: nada concurrent pode incluir e excluir
        //consumers.
    }

    #region channel management

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
            InitModel(model);
            ModelCreated(model);
        }
        catch (Exception ex)
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
            TerminateModel();
            model = connection!.CreateModel();
            InitModel(model);
            ModelReCreated(model, autorecovered);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to re-create a channel.");
            RetryReCreateChannel(autorecovered);
        }
    }

    private void RetryReCreateChannel(bool autorecovered)
    {
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            logger.LogInformation("Retrying to re-create a channel.");
            ReCreateModel(autorecovered);
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

    private void InitModel(IModel model)
    {
        model.ModelShutdown += onModelShutdowEventHander;
    }

    private void TerminateModel()
    {
        if (model is null)
            return;

        model.ModelShutdown -= onModelShutdowEventHander;
        model.Dispose();
        model = null;
    }

    private void OnModelShutdown(object? sender, ShutdownEventArgs e)
    {
        // if the connection was closed, then is not necessary to notify the consumers, either re-create the model.
        if (!modelCreated || !consumerStatus.IsConnected)
            return;

        logger.LogInformation("Channel shutdown. Notifying consumers.");

        foreach (var consumer in consumers)
        {
            try
            {
                consumer.ChannelClosed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed notifying (shutdown) for the consumer: {consumer).", consumer);
            }
        }

        RetryReCreateChannel(false);
    }

    #endregion

    #region IConnectionConsumer implementation


    void IConnectionConsumer.Closed()
    {
        logger.LogInformation("Connection closed. Notifying consumers.");

        TerminateModel();
        connection = null;
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

    private class ManagedConsumer : IChannelConsumerStatus
    {
        private readonly ManagedChannel managedChannel;
        private readonly IChannelConsumer consumer;

        public ManagedConsumer(ManagedChannel managedChannel, IChannelConsumer consumer)
        {
            this.managedChannel = managedChannel;
            this.consumer = consumer;
        }

        public bool IsOpen => managedChannel.IsOpen;

        public void Release()
        {
            managedChannel.Release(consumer);
        }
    }
}