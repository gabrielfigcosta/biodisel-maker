using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;
using System.Text;

namespace biodisel.queues.Publishers
{
    public class ReactorPublisher : PublisherBase<ReactorPublisher>
    {
        public ReactorPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<ReactorPublisher>> exchangeBindingOptions, ILogger<PublisherBase<ReactorPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public override void Send<T>(T message)
        {
            Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(message)));
        }
    }
}