using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class DryerPublisher : PublisherBase<DryerPublisher>
    {
        public DryerPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<DryerPublisher>> exchangeBindingOptions, ILogger<PublisherBase<DryerPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public override void Send<T>(T messageToSend)
        {
            Send( Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)) );
        }
    }
}