
using System.Runtime.Serialization;

namespace RoyalCode.RabbitMQ.Components.Declarations;

/// <inheritdoc/>
[Serializable]
public sealed class ChannelConfigurationException : InvalidOperationException
{
    /// <inheritdoc/>
    public ChannelConfigurationException(string message) : base(message) { }

    /// <inheritdoc/>
    public ChannelConfigurationException(string message, Exception innerException) : base(message, innerException) { }

    /// <inheritdoc/>
    private ChannelConfigurationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    { }
}