using System;
using System.Collections.Generic;
using System.Text;

namespace RoyalCode.RabbitMQ.Components.Connections;

public class ConnectionManager
{
    private readonly ConnectionPoolFactory connectionPoolFactory;

    public ConnectionManager(ConnectionPoolFactory connectionPoolFactory)
    {
        this.connectionPoolFactory = connectionPoolFactory;
    }


}

