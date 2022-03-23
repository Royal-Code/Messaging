using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Tests;

public sealed class ModelFactory : IDisposable
{
    private readonly TestChannelConsumer channelConsumer;
    private readonly TestConnectionConsumer connectionConsumer;
    private readonly IChannelConsumption consumption;
    
    public ModelFactory()
    {
        ServiceProvider = Container.Prepare();

        ConnectionManager = ServiceProvider.GetService<ConnectionManager>()!;
        connectionConsumer = new TestConnectionConsumer();
        ConnectionManager.Consume("test", connectionConsumer);
        
        ChannelManager = ServiceProvider.GetService<IChannelManager>()!;
        channelConsumer = new TestChannelConsumer();
        consumption = ChannelManager.Consume("test", channelConsumer);
    }

    public IServiceProvider ServiceProvider { get; }

    public ConnectionManager ConnectionManager { get; }
    
    public IChannelManager ChannelManager { get; }

    public IConnectionProvider ConnectionProvider => connectionConsumer.ConnectionProvider!;
    
    public IChannelProvider ChannelProvider => channelConsumer.ChannelProvider!;

    public IModel Model => ChannelProvider.GetSharedChannel();

    public void WaitForConnected(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var limit = DateTime.Now.Add(timeout.Value);
        while (true)
        {
            if ((connectionConsumer.ConnectionProvider?.IsOpen ?? false) && channelConsumer.IsConnected)
                return;

            if (DateTime.Now > limit)
                throw new TimeoutException();
            
            Thread.Sleep(1_500);
        }
    }
    
    public void Dispose()
    {
        consumption.Dispose();
    }
}