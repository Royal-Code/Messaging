using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <inheritdoc/>
public class ChannelConfiguraException : InvalidOperationException
{
    /// <inheritdoc/>
    public ChannelConfiguraException(string message) : base(message) { }

    /// <inheritdoc/>
    public ChannelConfiguraException(string message, Exception innerException) : base(message, innerException) { }
}