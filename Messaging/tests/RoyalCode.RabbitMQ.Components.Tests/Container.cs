using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class Container
{
    public static IServiceProvider Prepare(Action<IServiceCollection>? configure = null)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<Container>()
            .Build();



        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(builder => builder.AddConsole());

        services.AddRabbitMQComponents();
        if (configure is null)
        {
            services.ConfigureRabbitMQConnection("test", options =>
            {
                options.AddConnectionStringName("test");
            });
            services.ConfigureRabbitMQConnection("test", options =>
            {
                options.RetryConnectionDelay = TimeSpan.FromSeconds(1);
            });
        }
        else
        {
            configure.Invoke(services);    
        }
        
        var sp = services.BuildServiceProvider();

        return sp;
    }
}