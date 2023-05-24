using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Manage a channel (<see cref="IModel"/>) of the RabbitMQ.
/// </para>
/// </summary>
public abstract class ManagedChannel : IConnectionConsumer, IDisposable
{
    private readonly IConnectionConsumerStatus consumerStatus;
    private readonly List<IChannelConsumer> consumers = new();
    private readonly EventHandler<ShutdownEventArgs> onModelShutdowEventHander;
    private IConnection? connection;
    private IModel? model;

    private bool modelCreated;
    private bool disposing;
    private bool disposed;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger logger;

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
    ///     Event raised when the channel is re-created, by a reconnection of the connection, 
    ///     or re-creation of the channel.
    /// </para>
    /// <para>
    ///     The <see cref="bool"/> argument indicates whether the channel is re-created 
    ///     by auto-recovery of the connection.
    /// </para>
    /// </summary>
    public event EventHandler<bool>? OnReconnected;

    /// <summary>
    /// <para>
    ///     Consume a channel of RabbitMQ, object of type <see cref="IModel"/>.
    /// </para>
    /// </summary>
    /// <param name="consumer">The channel consumer.</param>
    /// <returns>A <ver cref="IDisposable"/> object to finalize the consumption.</returns>
    public virtual IChannelConsumerStatus Consume(IChannelConsumer consumer)
    {
        lock (consumers)
        {
            consumers.Add(consumer);
        }

#if NET5_0_OR_GREATER
        if (IsOpen)
#else
        if (IsOpen && model is not null)
#endif
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

    /// <summary>
    /// <para>
    ///     Release the RabbitMQ channel.
    /// </para> 
    /// <para>
    ///     The behavior of this method depends on the implementation of the channel strategy.
    ///     When the channel is released, the channel strategy will decide whether to close the channel or not.
    /// </para>
    /// <para>
    ///     The releasing occurs when the dispose method is called. 
    ///     If the channel is not released, the channel will not be closed and the connection consumer will not be released.
    /// </para>
    /// </summary>
    /// <returns>True if the channel is released, otherwise false.</returns>
    protected abstract bool ReleaseChannel();

    private void Release(IChannelConsumer consumer)
    {
        lock (consumers)
        {
            consumers.Remove(consumer);
        }
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

        logger.LogDebug("Channel created. Notifying currentConsumers.");

        lock (consumers)
        {
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
        logger.LogInformation("Channel re-created. Notifying currentConsumers.");

        lock (consumers)
        {
            foreach (var consumer in consumers)
            {
                try
                {
                    logger.LogDebug("Notifying that the model has been re-created for the consumer: {consumer}.", consumer);
                    consumer.Reloaded(model, autorecovered);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed notifying (model re-created) for the consumer: {consumer).", consumer);
                }
            }
            OnReconnected?.Invoke(this, autorecovered);
        }
    }

    private void InitModel(IModel model)
    {
        model.ModelShutdown += onModelShutdowEventHander;
    }

    /// <summary>
    /// Finaliza o canal.
    /// </summary>
    protected void TerminateModel(bool cleanConsumers = false)
    {
        if (model is null)
            return;

        model.ModelShutdown -= onModelShutdowEventHander;
        model.Dispose();
        model = null;

        if (cleanConsumers)
        {
            lock (consumers)
            {
                consumers.Clear();
            }
        }
    }

    /// <summary>
    /// <para>
    ///     Clean the events.
    /// </para>
    /// </summary>
    protected void CleanEvents()
    {
        OnReconnected = null;
    }

    private void OnModelShutdown(object? sender, ShutdownEventArgs e)
    {
        // if the connection was closed, then is not necessary to notify the currentConsumers, either re-create the model.
        if (!modelCreated || !consumerStatus.IsConnected || model is { IsOpen: true })
            return;

        logger.LogInformation("Channel shutdown and connection is open, then re-creating the channel.");

        RetryReCreateChannel(false);
    }

    #endregion

    #region IConnectionConsumer implementation

    void IConnectionConsumer.Disposing()
    {
        if (disposing || disposed)
            return;

        disposing = true;

        logger.LogInformation("Connection is disposing. Notifying currentConsumers.");

        IChannelConsumer[] currentConsumers;
        lock (consumers)
        {
            currentConsumers = consumers.ToArray();
        }

        foreach (var consumer in currentConsumers)
        {
            try
            {
                consumer.Disposing();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed notifying (closed) for the consumer: {consumer).", consumer);
            }
        }

        TerminateModel(true);
        connection = null;
        modelCreated = false;
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

    private sealed class ManagedConsumer : IChannelConsumerStatus
    {
        private readonly ManagedChannel managedChannel;
        private readonly IChannelConsumer consumer;

        public ManagedConsumer(ManagedChannel managedChannel, IChannelConsumer consumer)
        {
            this.managedChannel = managedChannel;
            this.consumer = consumer;
        }

        public bool IsOpen => managedChannel.IsOpen;

        public void Dispose()
        {
            managedChannel.Release(consumer);
        }
    }

    /// <summary>
    /// Execute the dispose.
    /// </summary>
    /// <param name="disposing">if are disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        var continueDispose = ReleaseChannel();
        if(!continueDispose)
            return;

        disposed = true;

        TerminateModel(true);

        connection = null;
        model = null;
        OnReconnected = null;

        modelCreated = false;
    }

    /// <summary>
    /// Finalize the channel.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}