using RoyalCode.Messaging.Abstractions.Exceptions;

namespace RoyalCode.Messaging.Abstractions.Handlers;

/// <summary>
/// <para>
///     Service to handle exception ocurred when publishing to multiples publishers (brokers).
/// </para>
/// </summary>
public interface IPublishExceptionHandler
{
    /// <summary>
    /// Clled when an exception occurs during publication.
    /// </summary>
    /// <param name="message">The published message.</param>
    /// <param name="exception">The exception occurred.</param>
    /// <typeparam name="TMessage">The message type.</typeparam>
    void ExceptionOccured<TMessage>(TMessage message, OnPublishException exception);
}

/// <summary>
/// <para>
///     Service to handle exception ocurred when publishing to multiples publishers (borkers).
/// </para>
/// </summary>
/// <typeparam name="TPublisher">The publisher type.</typeparam>
public interface IPublishExceptionHandler<TPublisher> : IPublishExceptionHandler { }