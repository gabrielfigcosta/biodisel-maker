using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using biodisel.domain.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using biodisel.domain.Models.Enums;
using RabbitMQ.Client.Core.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;
using biodisel.queues.Publishers;
using System;

namespace biodisel.queues.Consumers
{
    public class DryerConsumer : ConsumerBase<DryerConsumer>
    {

        private readonly ILogger<DryerConsumer> _logger;
        private readonly DryerPublisher _publisher;
        private readonly VolumePublisher _publisherReport;
        private decimal Volume { get; set; }
        public DryerConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<DryerConsumer>> queueOptions, ILogger<DryerConsumer> logger, DryerPublisher publisher, VolumePublisher publisherReport) : base(connectionOptions, queueOptions)
        {
            _logger = logger;
            _publisher = publisher;
            _publisherReport = publisherReport;
        }

        private async Task Publish(ResidueMessage message, string routingKey)
        {
            _logger.LogWarning($"Sending message to {routingKey}");
            await Task.Run(() => 
                _publisher.Send(
                    Encoding.Default.GetBytes(JsonConvert.SerializeObject(message)),
                    "biodisel-maker",
                    routingKey
                )
            );
            _logger.LogWarning($"Message to {routingKey} sent");
        }

        public override async void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );
            
            messageReceived.Volume -= messageReceived.Volume * 0.03m;
            messageReceived.IntegrationSystem = IntegrationSystem.Dryer;
            Volume = messageReceived.Volume;
            _publisherReport.SendVolumeReport(IntegrationSystem.Dryer,Volume);

            await Publish(messageReceived,"biodisel");
            await Task.Delay(TimeSpan.FromMilliseconds( 60 ));
            _publisherReport.SendVolumeReport(IntegrationSystem.Dryer,0);
            await Task.Delay(TimeSpan.FromSeconds( Convert.ToDouble(5.0m * Volume) ));
            Volume = 0;
        }
    }
}