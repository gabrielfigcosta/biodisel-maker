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
    public class OilTankPublisher : PublisherBase<OilTankPublisher>
    {
        public CancellationToken stoppingToken { get; set; }
        public OilTankPublisher(IOptions<RabbitMQConnectionOptions> connectionOptions, IOptions<ExchangeBindingOptions<OilTankPublisher>> exchangeBindingOptions, ILogger<PublisherBase<OilTankPublisher>> logger) : base(connectionOptions, exchangeBindingOptions, logger)
        {
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;
            while (!this.stoppingToken.IsCancellationRequested)
            {
                var messageToSend = new ResidueMessage{
                    IntegrationSystem = IntegrationSystem.Oil,
                    Volume = Convert.ToDecimal(new Random().NextDouble() + 1.0)
                };

                Send(messageToSend);

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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
            Send( Encoding.Default.GetBytes(JsonConvert.SerializeObject(messageToSend)) );
        }
    }
}