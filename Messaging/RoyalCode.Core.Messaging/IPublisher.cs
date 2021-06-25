using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Core.Messaging
{
    /// <summary>
    /// Componente de mensageria para publicar de mensagem em um determinado canal.
    /// </summary>
    /// <typeparam name="TMessage">Modelo de dado da mensagem.</typeparam>
    public interface IPublisher<TMessage> : IDisposable
    {
        /// <summary>
        /// Envia uma mensagem com os informações do modelo de dados.
        /// </summary>
        /// <param name="instance">Modelo de dados para enviar na mensagem.</param>
        /// <param name="token">Token para cancelamento da task.</param>
        Task PublishAsync(TMessage instance, CancellationToken token = default);
        
        /// <summary>
        /// Envia uma mensagem com os informações do modelo de dados para uma rota determinada.
        /// </summary>
        /// <param name="instance">Modelo de dados para enviar na mensagem.</param>
        /// <param name="routeKey">Chave para a rota.</param>
        /// <param name="token">Token para cancelamento da task.</param>
        Task PublishAsync(TMessage instance, string routeKey, CancellationToken token = default);
    }
}