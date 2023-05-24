using RoyalCode.RabbitMQ.Components.Connections;
using System;
using RoyalCode.RabbitMQ.Components.Channels;

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
        services.AddSingleton<IChannelManagerFactory, ChannelManagerFactory>();

        return services;
    }

    /// <summary>
    /// <para>
    ///     Configure a connection from a RabbitMQ Cluster for a given name.
    /// </para>
    /// <para>
    ///     The configuration is done over the <see cref="ConnectionPoolOptions"/>.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The RabbitMQ Cluster name.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The same instance of <paramref name="services"/> to chain calls.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref name="services"/> is null.
    /// </exception>
    public static IServiceCollection ConfigureRabbitMQConnection(this IServiceCollection services,
        string name, Action<ConnectionPoolOptions> configure)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services.Configure(name, configure);

        return services;
    }

    /// <summary>
    /// <para>
    ///     Configure a channel pool for a RabbitMQ Cluster.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The RabbitMQ Cluster name.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The same instance of <paramref name="services"/> to chain calls.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref name="services"/> is null.
    /// </exception>
    public static IServiceCollection ConfigureRabbitMQChannelPool(this IServiceCollection services,
        string name, Action<ChannelPoolOptions> configure)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services.Configure(name, configure);

        return services;
    }
}