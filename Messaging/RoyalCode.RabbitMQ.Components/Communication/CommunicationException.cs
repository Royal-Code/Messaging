
using System.Runtime.Serialization;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <inheritdoc/>
[Serializable]
public sealed class CommunicationException : InvalidOperationException
{
    /// <inheritdoc/>
    public CommunicationException(string message) : base(message) { }

    /// <inheritdoc/>
    public CommunicationException(string message, Exception innerException) : base(message, innerException) { }

    /// <inheritdoc/>
    private CommunicationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext) 
    { }
}
