using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Tests;

public sealed class ModelFactory : IDisposable
{
    private readonly TestChannelConsumer channelConsumer;
    private readonly TestConnectionConsumer connectionConsumer;
    private readonly IChannelConsumerStatus status;
    
    public ModelFactory()
    {
        ServiceProvider = Container.Prepare();

        ConnectionManager = ServiceProvider.GetService<ConnectionManager>()!;
        connectionConsumer = new TestConnectionConsumer();
        ConnectionManager.Consume("test", connectionConsumer);
        
        Factory = ServiceProvider.GetService<IChannelManagerFactory>()!;
        ChannelManager = Factory.GetChannelManager("test");
        channelConsumer = new TestChannelConsumer();
        ManagedChannel= ChannelManager.CreateChannel();
        status = ManagedChannel.Consume(channelConsumer);
    }

    public IServiceProvider ServiceProvider { get; }

    public ConnectionManager ConnectionManager { get; }
    
    public IChannelManagerFactory Factory { get; }

    public IChannelManager ChannelManager { get; }
    
    public ManagedChannel ManagedChannel { get; }

    public IConnection? Connection => connectionConsumer.Connection;

    public IModel Model => ManagedChannel.Channel ?? throw new Communication.CommunicationException("Not connected");

    public ILogger<T> CreateLogger<T>() => ServiceProvider.GetRequiredService<ILogger<T>>()!;

    public void WaitForConnected(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var limit = DateTime.Now.Add(timeout.Value);
        while (true)
        {
            if ((connectionConsumer.Connection?.IsOpen ?? false) && ManagedChannel.IsOpen)
                return;

            if (DateTime.Now > limit)
                throw new TimeoutException();
            
            Thread.Sleep(1_500);
        }
    }
    
    public void Dispose()
    {
        status.Dispose();
        ManagedChannel.Dispose();
    }
}