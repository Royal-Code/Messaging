
using RoyalCode.RabbitMQ.Components.Connections;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <para>
///     Extensions methods for <see cref="IServiceCollection"/>.
/// </para>
/// </summary>
public static class RabbitMqComponentsServiceCollectionExtensions
{

    public static IServiceCollection AddRabbitMQComponents(this IServiceCollection services)
    {

        services.AddSingleton<ConnectionManager>();
        services.AddSingleton<ConnectionPoolFactory>();

        return services;
    }
}
