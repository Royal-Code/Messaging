namespace RoyalCode.Messaging.Abstractions.Handlers;

/// <summary>
/// <para>
///     Factory to create the exception handlers.
/// </para>
/// </summary>
public interface IExceptionHandlersFactory
{
    /// <summary>
    /// Gets or create the handlers for publication exception.
    /// </summary>
    /// <param name="publisherType">The type of the publisher.</param>
    /// <returns>An enumerable.</returns>
    IEnumerable<IPublishExceptionHandler> GetPublishExceptionHandler(Type publisherType);
}