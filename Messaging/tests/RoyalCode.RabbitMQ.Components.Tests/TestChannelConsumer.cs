using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class TestChannelConsumer : IChannelConsumer
{
    public IChannelProvider? ChannelProvider { get; private set; }
    
    public bool IsConnected { get; private set; }
    
    public void Consume(IChannelProvider provider)
    {
        ChannelProvider = provider;
        IsConnected = true;
    }

    public void ConnectionClosed()
    {
        IsConnected = false;
    }

    public void ConnectionRecovered(bool autorecovered)
    {
        IsConnected = true;
    }
}