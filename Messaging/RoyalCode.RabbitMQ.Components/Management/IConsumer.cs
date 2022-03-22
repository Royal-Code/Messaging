namespace RoyalCode.RabbitMQ.Components.Management;

public interface IConsumer<T>
{
    
    void Consume(IProvider<T> provider);

    
    void Reconnected(bool autorecovered);

    
    void Closed();
}