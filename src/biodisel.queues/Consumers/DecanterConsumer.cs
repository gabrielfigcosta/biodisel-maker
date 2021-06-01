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
    public class DecanterConsumer : ConsumerBase<DecanterConsumer>
    {

        private readonly ILogger<DecanterConsumer> _logger;
        private readonly DecanterPublisher _publisher;
        private readonly VolumePublisher _publisherReport;
        private decimal Volume { get; set; }
        private int MaximumLiters = 10;

        public DecanterConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<DecanterConsumer>> queueOptions, ILogger<DecanterConsumer> logger, DecanterPublisher publisher, VolumePublisher publisherReport) : base(connectionOptions, queueOptions)
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

        private async Task SendGlycerin(decimal volume)
        {
            var glycerinVolume = volume * 0.02m;
            var glycerinMessage = new ResidueMessage
            {
                Volume = glycerinVolume,
                IntegrationSystem = IntegrationSystem.Decanter
            };
            await Publish(glycerinMessage,"glycerin");
            Volume -= glycerinVolume;
            await SendVolumeReport();
        }

        private async Task SendEthanol(decimal volume)
        {
            var ethanolVolume = volume * 0.09m;
            var ethanolMessage = new ResidueMessage
            {
                Volume = ethanolVolume,
                IntegrationSystem = IntegrationSystem.Ethanol
            };
            await Publish(ethanolMessage,"ethanol");
            Volume -= ethanolVolume;
            await SendVolumeReport();
        }

        private async Task SendWashSolution(decimal volume)
        {
            var washSolutionVolume = volume * 0.89m;
            var washMessage = new ResidueMessage
            {
                Volume = washSolutionVolume,
                IntegrationSystem = IntegrationSystem.Decanter
            };
            await Publish(washMessage,"washer");
            Volume = Math.Max(Volume - washSolutionVolume,0);
            await SendVolumeReport();
        }

        private async Task SendVolumeReport()
        {
            await Task.Run(() => 
                _publisherReport.SendVolumeReport(IntegrationSystem.Decanter,Volume)
            );
        }

        public override async void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );
            _logger.LogInformation($"Message received from {messageReceived.IntegrationSystem} with volume {messageReceived.Volume}");

            if (messageReceived.Volume > MaximumLiters)
            {
                _logger.LogWarning($"Using only 10 liters, of {messageReceived.Volume} received");
                messageReceived.Volume = 10;
            }
            
            Volume = messageReceived.Volume;
            await SendVolumeReport();
            await SendGlycerin(messageReceived.Volume);
            await SendEthanol(messageReceived.Volume);
            await SendWashSolution(messageReceived.Volume);
        }
    }
}