namespace RoyalCode.RabbitMQ.Components.Channels;

public interface IChannelManager
{
    void Consume(string name, ChannelStrategy strategy, IChannelConsumer consumer);
}