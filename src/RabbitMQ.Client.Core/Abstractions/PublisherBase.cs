using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;

namespace RabbitMQ.Client.Core.Abstractions
{
    public abstract class PublisherBase<TPublisher> where TPublisher : class
    {
        protected ConnectionFactory ConnectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        protected readonly ExchangeBindingOptions<TPublisher> _exchangeBindingOptions;
        private readonly ILogger<PublisherBase<TPublisher>> _logger;
        protected bool Verbose = false;

        public PublisherBase(
            IOptions<RabbitMQConnectionOptions> connectionOptions,
            IOptions<ExchangeBindingOptions<TPublisher>> exchangeBindingOptions, 
            ILogger<PublisherBase<TPublisher>> logger)
        {
            ConnectionFactory = new ConnectionFactory
            {
                HostName = connectionOptions.Value.HostName,
                Port = connectionOptions.Value.Port,
                UserName = connectionOptions.Value.UserName,
                Password = connectionOptions.Value.Password,
                VirtualHost = connectionOptions.Value.VirtualHost
            };
            _connection = ConnectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _exchangeBindingOptions = exchangeBindingOptions.Value;
            _logger = logger;
        }

        public void Send(byte[] message)
        {
            if(Verbose)
                _logger.LogInformation($"Sending message to exchange {_exchangeBindingOptions.Exchange} " + 
                                       $"it routing key is {_exchangeBindingOptions.RoutingKey}");
            _channel.BasicPublish(
                _exchangeBindingOptions.Exchange,
                _exchangeBindingOptions.RoutingKey,
                null,
                body: message
            );
            if(Verbose)
                _logger.LogInformation($"Message sent from {typeof(TPublisher).Name}");
        }
        
        public void Send(byte[] message,string exchange, string routingKey)
        {
            if(Verbose)
                _logger.LogInformation($"Sending message to exchange {exchange} " + 
                                   $"it routing key is {routingKey}");
            _channel.BasicPublish(
                exchange,
                routingKey,
                null,
                body: message
            );
            if(Verbose)
                _logger.LogInformation($"Message sent from {typeof(TPublisher).Name}");
        }

        public abstract void Send<T>(T messageToSend);
    }
}
