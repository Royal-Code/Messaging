namespace RoyalCode.Messaging.Abstractions.Exceptions;

/// <summary>
/// <para>
///     Exception for when there is no configuration for receive messages. 
/// </para>
/// </summary>
public class ReceiverNotConfiguredException : Exception
{
    /// <summary>
    /// Creates a new exception for publisher not configured for <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Not configured message type.</param>
    public ReceiverNotConfiguredException(Type type)
        : base(string.Format(Resources.ReceiverNotConfigured, type.Name))
    { }
}