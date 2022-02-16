﻿
using RoyalCode.RabbitMQ.Components.Connections;
using System;

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
    public static IServiceCollection ConfigureRabbitMQConnection(this IServiceCollection services,
        string name, Action<ConnectionPoolOptions> configure)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services.Configure(name, configure);

        return services;
    }
}
