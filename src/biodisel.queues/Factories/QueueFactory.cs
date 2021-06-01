using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;
using Microsoft.Extensions.Logging;
using biodisel.domain.Interfaces;

namespace biodisel.queues.Factories
{
    public class QueueFactory : IQueueFactory
    {
        private readonly ConnectionFactory connectionFactory;
        private readonly ILogger<QueueFactory> _logger;

        public QueueFactory(IOptions<RabbitMQConnectionOptions> rabbitOptions, ILogger<QueueFactory> logger)
        {
            connectionFactory = new ConnectionFactory
            {
                HostName = rabbitOptions.Value.HostName,
                VirtualHost = rabbitOptions.Value.VirtualHost,
                Port = rabbitOptions.Value.Port,
                UserName = rabbitOptions.Value.UserName,
                Password = rabbitOptions.Value.Password
            };
            _logger = logger;
        }


        public void CreateDefaultAndBind(
            string queueName,
            string routingKey, string exchange)
        {
            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queueName,true,false,false,null);
            _logger.LogDebug($"Queue {queueName} declared!");
            channel.QueueBind(queue: queueName,routingKey: routingKey,exchange: exchange);
            _logger.LogDebug($"{queueName} binded to {exchange} with routing key as {routingKey}");
        }
    }
}