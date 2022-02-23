using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Component that handle the complexity to manage channel and connection and provides
///     the channel to communicate with the RabbitMQ.
/// </para>
/// <para>
///     This component is similiar of <see cref="IChannelProvider"/>, 
///     but here the channels are managed by a pool, and there will be a limit to the simultaneous use of channels.
/// </para>
/// </summary>
public interface IPooledChannelProvider
{
    /// <summary>
    /// Gets a <see cref="IPooledModel"/>, that contains the channel, from the pool.
    /// </summary>
    /// <returns></returns>
    Task<IPooledModel> GetAsync();
}

public class PooledChannelProvider : IPooledChannelProvider
{
    private readonly ObjectPool<IModel> modelPool;

    public PooledChannelProvider()
    {
        var p = new DefaultObjectPoolProvider();
        modelPool = p.Create(new ModelPooledObjectPolicy());
    }

    public Task<IPooledModel> GetAsync()
    {
        

        modelPool.Get();

        throw new System.NotImplementedException();
    }
}

public class ModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
{


    public IModel Create()
    {
        throw new System.NotImplementedException();
    }

    public bool Return(IModel obj)
    {
        throw new System.NotImplementedException();
    }
}