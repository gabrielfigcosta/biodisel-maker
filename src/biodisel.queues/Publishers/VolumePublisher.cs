using System.Text;
using biodisel.domain.Models;
using biodisel.domain.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class VolumePublisher : PublisherBase<VolumePublisher>
    {
        public VolumePublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<VolumePublisher>> exchangeBindingOptions, ILogger<PublisherBase<VolumePublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public void SendVolumeReport(IntegrationSystem integration, decimal volume)
        {
            var messageToSend = new ResidueMessage{
                    IntegrationSystem = integration,
                    Volume = volume
            };

            Send<ResidueMessage>(messageToSend);
        }
        
        public override void Send<T>(T messageToSend)
        {
            Send( Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)) );
        }
    }
}