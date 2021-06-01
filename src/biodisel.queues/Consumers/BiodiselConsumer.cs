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
    public class BiodiselConsumer : ConsumerBase<BiodiselConsumer>
    {

        private readonly ILogger<BiodiselConsumer> _logger;
        private readonly VolumePublisher _publisherReport;
        private decimal Volume { get; set; } = 0;
        public BiodiselConsumer(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<QueueOptions<BiodiselConsumer>> queueOptions, ILogger<BiodiselConsumer> logger, VolumePublisher publisherReport) : base(connectionOptions, queueOptions)
        {
            _logger = logger;
            _publisherReport = publisherReport;
        }

        private async Task SendVolumeReport()
        {
            await Task.Run(() => 
                _publisherReport.SendVolumeReport(IntegrationSystem.Biodisel,Volume)
            );
        }

        public override async void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );
            if(messageReceived.IntegrationSystem == IntegrationSystem.Dryer)
                Volume += messageReceived.Volume;
            _logger.LogDebug($"Recebi do {messageReceived.IntegrationSystem} volume de {messageReceived.Volume}");
            await SendVolumeReport();
        }
    }
}