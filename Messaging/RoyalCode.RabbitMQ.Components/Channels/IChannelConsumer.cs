namespace RoyalCode.RabbitMQ.Components.Channels;

public interface IChannelConsumer
{

    void Consume(IChannelProvider provider);

    void ConnectionClosed();

    void ConnectionRecovered(bool autorecovered);
}