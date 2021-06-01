using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.Options;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Client.Core.Abstractions
{
    public abstract class ConsumerAsyncBase<TConsumer> : BackgroundService where TConsumer : class
    {
        protected ConnectionFactory ConnectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly QueueOptions<TConsumer> _queueOptions;
        protected string Id;

        public ConsumerAsyncBase(IOptions<RabbitMQConnectionOptions> connectionOptions,
            IOptions<QueueOptions<TConsumer>> queueOptions)
        {
            ConnectionFactory = new ConnectionFactory
            {
                HostName = connectionOptions.Value.HostName,
                Port = connectionOptions.Value.Port,
                UserName = connectionOptions.Value.UserName,
                Password = connectionOptions.Value.Password,
                VirtualHost = connectionOptions.Value.VirtualHost,
                DispatchConsumersAsync = true
            };
            _connection = ConnectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _queueOptions = queueOptions.Value;
            Id = $"{Environment.UserName}-{typeof(TConsumer).Name}@{Guid.NewGuid()}";

            Setup();
        }

        private void Initialize()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnReceived;
            consumer.Registered += Consumer_Registered;
            consumer.Unregistered += Consumer_Unregistered;
            _channel.BasicConsume(
               _queueOptions.QueueName,
               _queueOptions.AutoAck,
               consumer
           );
        }

        private Task Consumer_Unregistered(object sender, ConsumerEventArgs @event)
        {
            Console.WriteLine($"Unregister {Id} -> {@event.ConsumerTags.Aggregate((a, b) => $"{a};{b}")}");
            return Task.CompletedTask;
        }

        private Task Consumer_Registered(object sender, ConsumerEventArgs @event)
        {
            Console.WriteLine($"Register {Id} -> {@event.ConsumerTags.Aggregate((a, b) => $"{a};{b}")}");
            return Task.CompletedTask;
        }

        public abstract Task OnReceived(object sender, BasicDeliverEventArgs @event);

        protected void Setup()
        {
            _channel.QueueDeclare(
                queue: _queueOptions.QueueName,
                durable: _queueOptions.Durable,
                exclusive: _queueOptions.Exclusive,
                autoDelete: _queueOptions.AutoDelete
            );

            SetupExchangeBindings(_channel, _queueOptions);
        }

        protected void SetupExchangeBindings(IModel channel, QueueOptions<TConsumer> queueOptions)
        {
            foreach (var exchangeToBind in queueOptions.Bindings)
            {
                _channel.QueueBind(
                    exchange: exchangeToBind.Exchange,
                    queue: queueOptions.QueueName,
                    routingKey: exchangeToBind.RoutingKey
                );
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Initialize();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }
    }
}
