using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using biodisel.domain.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Core.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;
using biodisel.Web.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace biodisel.queues.Consumers
{
    public class TransactionConsumer : ConsumerBase<TransactionConsumer>
    {

        private readonly ILogger<TransactionConsumer> _logger;
        private readonly IHubContext<SimulatorHub> _hubContext;

        public TransactionConsumer(ILogger<TransactionConsumer> logger,
                               IOptions<RabbitMQConnectionOptions> connectionOptions,
                               IOptions<QueueOptions<TransactionConsumer>> queueOptions, IHubContext<SimulatorHub> hubContext) : base(connectionOptions, queueOptions)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public override async void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageReceived = JsonConvert.DeserializeObject<ResidueMessage>(
                Encoding.Default.GetString(eventArgs.Body.ToArray())
            );
            _logger.LogInformation($"Message received from {messageReceived.IntegrationSystem} with volume {messageReceived.Volume} on {nameof(TransactionConsumer)}");
            await _hubContext.Clients.All.SendAsync("ReceiveMessage",messageReceived.IntegrationSystem,messageReceived.Volume);
        }
    }
}