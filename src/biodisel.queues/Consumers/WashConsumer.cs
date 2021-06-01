using System.Threading.Tasks;
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
using System;

namespace biodisel.queues.Consumers
{
    public class WashConsumer: ConsumerBase<WashConsumer>
    {

        private readonly WashPublisher _publisher;
        private readonly VolumePublisher _publisherReport;
        private readonly ILogger<WashConsumer> _logger;
        private IntegrationSystem[] WashTanks = new IntegrationSystem[]{
            IntegrationSystem.FirstWashTank,
            IntegrationSystem.SecondWashTank,
            IntegrationSystem.ThirdWashTank,
        };
        public WashConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<WashConsumer>> queueOptions, WashPublisher publisher, ILogger<WashConsumer> logger, VolumePublisher publisherReport) : base(connectionOptions, queueOptions)
        {
            _publisher = publisher;
            _logger = logger;
            _publisherReport = publisherReport;
        }

        public override async void OnReceived(object sender,BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );

            _logger.LogInformation($"Message received from {messageReceived.IntegrationSystem} with volume {messageReceived.Volume}");

            if (messageReceived.IntegrationSystem == IntegrationSystem.Decanter)
            {
                messageReceived.IntegrationSystem = IntegrationSystem.Wash;
                for (int i = 0; i < 3; i++)
                {
                    messageReceived.Volume -= messageReceived.Volume * 0.075m;
                    _publisherReport.SendVolumeReport(WashTanks[i],messageReceived.Volume);
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    _publisherReport.SendVolumeReport(WashTanks[i],0);
                }
                _publisher.Send<ResidueMessage>(messageReceived);
            }
        }
    }
}