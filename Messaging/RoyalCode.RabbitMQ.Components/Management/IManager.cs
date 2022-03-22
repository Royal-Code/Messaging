namespace RoyalCode.RabbitMQ.Components.Management;

public interface IManager<T>
{
    IConsumption Consume(string name, IConsumer<T> consumer);
}