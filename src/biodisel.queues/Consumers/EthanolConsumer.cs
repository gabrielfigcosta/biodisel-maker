using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using biodisel.domain.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using biodisel.domain.Models.Enums;
using biodisel.queues.Publishers;
using RabbitMQ.Client.Core.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Consumers
{
    public class EthanolConsumer: ConsumerBase<EthanolConsumer>
    {
        private EthanolPublisher _publisher;
        private readonly ILogger<ReactorConsumer> _logger;

        public EthanolConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<EthanolConsumer>> queueOptions, EthanolPublisher publisher, ILogger<ReactorConsumer> logger) : base(connectionOptions, queueOptions)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public override void OnReceived(object sender,BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );

            _logger.LogInformation($"Message received from {messageReceived.IntegrationSystem} with volume {messageReceived.Volume}");

            if (messageReceived.IntegrationSystem == IntegrationSystem.Decanter)
            {
                messageReceived.IntegrationSystem = IntegrationSystem.Ethanol;
                _publisher.Send<ResidueMessage>(messageReceived);
            }
        }
    }
}