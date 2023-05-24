using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Channels;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class TestChannelConsumer : IChannelConsumer
{
    public IModel? Model { get; private set; }

    public void Consume(IModel channel)
    {
        Model = channel;
    }

    public void Disposing()
    {
        Model = null;
    }

    public void Reloaded(IModel channel, bool autorecovered)
    {
        Model = channel;
    }
}