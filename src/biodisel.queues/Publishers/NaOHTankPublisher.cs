using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using biodisel.domain.Models;
using biodisel.domain.Models.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.Abstractions;
using RabbitMQ.Client.Core.Options;

namespace biodisel.queues.Publishers
{
    public class NaOHTankPublisher : PublisherBase<NaOHTankPublisher>
    {
        public CancellationToken stoppingToken {get; set;}
        public NaOHTankPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<NaOHTankPublisher>> exchangeBindingOptions, ILogger<PublisherBase<NaOHTankPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            stoppingToken = token;
            while (!stoppingToken.IsCancellationRequested)
            {
                var messageToSend = new ResidueMessage{
                    IntegrationSystem = IntegrationSystem.NaOH,
                    Volume = 0.25m
                };

                Send(messageToSend);

                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        public Task StopAsync(CancellationToken cancellation)
        {
            if(cancellation.IsCancellationRequested)
                stoppingToken = cancellation;
            return Task.CompletedTask;
        }

        public override void Send<T>(T messageToSend)
        {
            Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)));
        }
    }
}