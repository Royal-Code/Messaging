using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <inheritdoc/>
public class CommunicationException : InvalidOperationException
{
    /// <inheritdoc/>
    public CommunicationException(string message) : base(message) { }

    /// <inheritdoc/>
    public CommunicationException(string message, Exception innerException) : base(message, innerException) { }
}
