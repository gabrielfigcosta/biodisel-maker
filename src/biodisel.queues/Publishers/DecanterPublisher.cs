using System.Text;
using biodisel.domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class DecanterPublisher : PublisherBase<DecanterPublisher>
    {
        public DecanterPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<DecanterPublisher>> exchangeBindingOptions, ILogger<PublisherBase<DecanterPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public override void Send<T>(T messageToSend)
        {
            Send( Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)) );
        }
    }
}