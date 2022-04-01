
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.Messaging;
using RoyalCode.Messaging.Abstractions.Bus;
using RoyalCode.Messaging.Abstractions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class MessagingAbstractionServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core services for messaging libraries.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The same instance of <paramref name="services"/> for chained calls.</returns>
    public static IServiceCollection AddMessagingCore(this IServiceCollection services)
    {
        services.TryAddTransient<IMessageBus, MessageBus>();
        services.TryAddTransient<PublisherFactory>();
        services.TryAddTransient<ReceiverFactory>();

        return services;
    }

    /// <summary>
    /// Configure options for messaging (<see cref="MessagingOptions"/>).
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configure">The configure action.</param>
    /// <returns>The same instance of <paramref name="services"/> for chained calls.</returns>
    public static IServiceCollection ConfigureMessagingOptions(
        this IServiceCollection services,
        Action<MessagingOptions> configure) => services.Configure(configure);
}