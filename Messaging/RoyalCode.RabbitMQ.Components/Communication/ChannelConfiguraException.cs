using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <inheritdoc/>
public class ChannelConfigurationException : InvalidOperationException
{
    /// <inheritdoc/>
    public ChannelConfigurationException(string message) : base(message) { }

    /// <inheritdoc/>
    public ChannelConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}