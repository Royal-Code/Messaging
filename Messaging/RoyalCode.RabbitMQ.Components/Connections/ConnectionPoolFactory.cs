using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoyalCode.RabbitMQ.Components.Connections
{
    internal class ConnectionPoolFactory
    {
        private readonly IOptionsMonitor<ConnectionPoolOptions> options;

        public ConnectionPoolFactory(IOptionsMonitor<ConnectionPoolOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));


        }
    }
}
