using System.Threading;
using System.Threading.Tasks;
using biodisel.domain.Interfaces;
using biodisel.queues.Consumers;
using biodisel.queues.Publishers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Core.Interfaces;

namespace biodisel.core
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ReactorConsumer _consumerReactor;
        private readonly GlycerinConsumer _consumerGlycerin;
        private readonly WashConsumer _consumerWasher;
        private readonly DecanterConsumer _consumerDecanter;
        private readonly NaOHTankPublisher _publisherNaOH;
        private readonly EthanolPublisher _publisherEthanol;
        private readonly DecanterPublisher _publisherDecanter;
        private readonly OilTankPublisher _publisherOil;
        private readonly IExchangesConfigurator _exchangesConfigurator;
        private readonly IQueueFactory _queueFactory;

        public CancellationToken token;

        public Worker(
            ILogger<Worker> logger,
            IExchangesConfigurator exchangesConfigurator,
            ReactorConsumer consumerReactor,
            NaOHTankPublisher publisherNaOH,
            EthanolPublisher publisherEthanol,
            OilTankPublisher publisherOil,
            DecanterPublisher publisherDecanter,
            DecanterConsumer consumerDecanter,
            WashConsumer consumerWasher,
            IQueueFactory queueFactory, GlycerinConsumer consumerGlycerin)
        {
            _logger = logger;
            _exchangesConfigurator = exchangesConfigurator;
            _consumerReactor = consumerReactor;
            _publisherNaOH = publisherNaOH;
            _publisherEthanol = publisherEthanol;
            _publisherOil = publisherOil;
            _publisherDecanter = publisherDecanter;
            _consumerDecanter = consumerDecanter;
            _consumerWasher = consumerWasher;
            _queueFactory = queueFactory;
            _consumerGlycerin = consumerGlycerin;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(!cancellationToken.IsCancellationRequested)
                token = cancellationToken;
                ExecuteAsync();
            return Task.CompletedTask;
        }

        protected void StopConsumers()
        {
            _consumerDecanter.StopAsync(token);
            _consumerGlycerin.StopAsync(token);
            _consumerReactor.StopAsync(token);
            _consumerWasher.StopAsync(token);
        }

        public void StopPublishers()
        {
            _publisherNaOH.StopAsync(token);
            _publisherEthanol.StopAsync(token);
            _publisherOil.StopAsync(token);
        }

        public Task StartPublishers()
        {
            _publisherNaOH.ExecuteAsync(token);
            _publisherEthanol.ExecuteAsync(token);
            _publisherOil.ExecuteAsync(token);
            return Task.CompletedTask;
        }

        protected async Task ExecuteAsync()
        {
            StartPublishers();
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000, token);
            }
            StopPublishers();
            StopConsumers();
        }
        

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested){
                _logger.LogDebug("Parando tuto");
                token = cancellationToken;
            }
            return Task.CompletedTask;
        }
    }
}
