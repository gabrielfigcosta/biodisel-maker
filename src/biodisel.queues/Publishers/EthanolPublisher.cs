using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using biodisel.domain.Models;
using biodisel.domain.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class EthanolPublisher : PublisherBase<EthanolPublisher>
    {
        public CancellationToken cancellationToken { get; set; }
        public EthanolPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<EthanolPublisher>> exchangeBindingOptions, ILogger<PublisherBase<EthanolPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public override void Send<T>(T messageToSend)
        {
            Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)));
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cancellationToken = stoppingToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                var messageToSend = new ResidueMessage{
                    IntegrationSystem = IntegrationSystem.Ethanol,
                    Volume = 0.125m
                };
                Send(messageToSend);

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        public Task StopAsync(CancellationToken cancellation)
        {
            if(cancellation.IsCancellationRequested)
                cancellationToken = cancellation;
            return Task.CompletedTask;
        }
    }
}