using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using RoyalCode.RabbitMQ.Components.ObjectPool;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Internal default implementation of <see cref="IChannelProvider"/>.
/// </para>
/// </summary>
internal sealed class DefaultChannelProvider : IChannelProvider, IDisposable
{
    private IModel? sharedModel;
    private readonly IAsyncObjectPool<IModel> pool;
    private readonly IConnectionProvider connectionProvider;
    private readonly ILogger logger;

    /// <summary>
    /// Creates a new channel provider.
    /// </summary>
    /// <param name="connectionProvider">The connection provider.</param>
    /// <param name="options">The channel options</param>
    /// <param name="logger">The logger.</param>
    public DefaultChannelProvider(
        IConnectionProvider connectionProvider, 
        ChannelPoolOptions options,
        ILogger logger)
    {
        this.connectionProvider = connectionProvider;
        this.logger = logger;

        var policy = new ObjectPoolPolicy<IModel>(options.PoolMaxSize, CreateChannel, InitModel, ResetModel);
        pool = new DefaultAsyncObjectPool<IModel>(policy);
    }

    /// <inheritdoc />
    public async Task<IModel> GetPooledChannelAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting a pooled channel");
        
        var pooled = await pool.GetAsync(cancellationToken);
        
        logger.LogInformation("Pooled channel received");
        
        return pooled.Instace;
    }

    /// <inheritdoc />
    public void ReturnPooledChannel(IModel model)
    {
        logger.LogInformation("Returning a pooled channel");
        
        pool.Return(model);
    }

    /// <inheritdoc />
    public IModel CreateChannel()
    {
        if (!connectionProvider.IsOpen)
        {
            logger.LogInformation("Can not create a RabbitMQ Channel because the connection is not open");
            throw new CanCreateChannelException("Connection is closed.");
        }

        try
        {
            logger.LogInformation("Creating a RabbitMQ Channel");
            return connectionProvider.Connection.CreateModel();
        }
        catch (Exception e)
        {
            const string msg = "An error occurred while trying to create a channel with RabbitMQ";
            logger.LogError(e, msg);
            throw new CanCreateChannelException(msg, e);
        }
    }

    /// <inheritdoc />
    public IModel GetSharedChannel()
    {
        return sharedModel ??= SafeCreateModel();
    }
    
    private IModel SafeCreateModel()
    {
        lock (pool)
        {
            return sharedModel ??= SafeCreateModel();
        }
    }

    private void InitModel(IModel model) { }
    private void ResetModel(IModel model) { }

    public void Dispose() => connectionProvider.Dispose();
}