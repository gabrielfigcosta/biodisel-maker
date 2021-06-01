using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using biodisel.domain.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using biodisel.domain.Interfaces;
using RabbitMQ.Client.Core.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;
using System.Threading.Tasks;

namespace biodisel.queues.Consumers
{
    public class ReactorConsumer : ConsumerAsyncBase<ReactorConsumer>
    {

        private readonly ILogger<ReactorConsumer> _logger;
        private readonly IReactorService _service;

        public ReactorConsumer(ILogger<ReactorConsumer> logger, IReactorService service,
                               IOptions<RabbitMQConnectionOptions> connectionOptions,
                               IOptions<QueueOptions<ReactorConsumer>> queueOptions) : base(connectionOptions,queueOptions)
        {
            _logger = logger;
            _service = service;
        }

        public override async Task OnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(@event.Body.ToArray())
            );
            _logger.LogInformation($"Message received from {messageReceived.IntegrationSystem} with volume {messageReceived.Volume} on {nameof(ReactorConsumer)}");
            await _service.Fill(messageReceived);
        }
    }
}