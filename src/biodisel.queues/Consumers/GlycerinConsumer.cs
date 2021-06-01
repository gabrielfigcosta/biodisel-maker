using System;
using System.Text;
using biodisel.domain.Models;
using biodisel.domain.Models.Enums;
using biodisel.queues.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;
using RabbitMQ.Client.Events;

namespace biodisel.queues.Consumers
{
    public class GlycerinConsumer : ConsumerBase<GlycerinConsumer>
    {
        private readonly ILogger<GlycerinConsumer> _logger;
        private decimal Volume = 0;
        private readonly VolumePublisher _publisher;

        public GlycerinConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<GlycerinConsumer>> queueOptions, ILogger<GlycerinConsumer> logger, VolumePublisher publisher) : base(connectionOptions, queueOptions)
        {
            _logger = logger;
            _publisher = publisher;
        }

        public override void OnReceived(object sender,BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );

            if (messageReceived.IntegrationSystem == IntegrationSystem.Decanter)
            {
                Volume += messageReceived.Volume;
                _publisher.SendVolumeReport(IntegrationSystem.Glycerin,Volume);
            }
        }
    }
}