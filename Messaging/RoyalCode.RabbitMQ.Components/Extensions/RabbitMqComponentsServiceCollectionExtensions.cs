
using RoyalCode.RabbitMQ.Components.Connections;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <para>
///     Extensions methods for <see cref="IServiceCollection"/>.
/// </para>
/// </summary>
public static class RabbitMqComponentsServiceCollectionExtensions
{
    /// <summary>
    /// Add services to use RabbitMQ Components.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same instance of <paramref name="services"/> to chain calls.</returns>
    public static IServiceCollection AddRabbitMQComponents(this IServiceCollection services)
    {

        services.AddSingleton<ConnectionManager>();
        services.AddSingleton<ConnectionPoolFactory>();

        return services;
    }
}
