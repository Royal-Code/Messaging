namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// Exception thrown when it is not possible to create channels with RabbitMQ
/// </summary>
public class CanCreateChannelException : InvalidOperationException
{
    /// <inheritdoc />
    public CanCreateChannelException(string? message) : base(message) { }

    /// <inheritdoc />
    public CanCreateChannelException(string? message, Exception? innerException) : base(message, innerException) { }
}