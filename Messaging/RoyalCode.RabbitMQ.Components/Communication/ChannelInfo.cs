using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

public class ChannelInfo
{
    internal void ConnectionRecreated()
    {
        throw new NotImplementedException();
    }

    internal PublicationAddress GetPublicationAddress(IModel model)
    {
        throw new NotImplementedException();
    }
}
