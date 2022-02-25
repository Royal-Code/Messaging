namespace RoyalCode.RabbitMQ.Components.Channels;

public interface IChannelManager
{
    void Consume(string name, IChannelConsumer consumer);
}