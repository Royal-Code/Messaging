using System;

namespace RoyalCode.RabbitMQ.Components.Management;

public interface IConsumption : IDisposable
{
    bool IsConnectionOpen { get; }
}