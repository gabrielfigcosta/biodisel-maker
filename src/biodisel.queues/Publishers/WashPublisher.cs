using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class WashPublisher : PublisherBase<WashPublisher>
    {
        public WashPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<WashPublisher>> exchangeBindingOptions, ILogger<PublisherBase<WashPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public override void Send<T>(T messageToSend)
        {
            Send( Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)) );
        }
    }
}