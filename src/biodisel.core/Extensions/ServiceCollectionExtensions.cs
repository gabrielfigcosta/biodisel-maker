using biodisel.application.Services;
using biodisel.domain.Interfaces;
using biodisel.queues.Consumers;
using biodisel.queues.Factories;
using biodisel.queues.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Core.Extensions;

namespace biodisel.core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPublishersExchanges(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPublisherExchange<ReactorPublisher>(configuration);
            services.AddPublisherExchange<NaOHTankPublisher>(configuration);
            services.AddPublisherExchange<EthanolPublisher>(configuration);
            services.AddPublisherExchange<OilTankPublisher>(configuration);
            services.AddPublisherExchange<WashPublisher>(configuration);
            services.AddPublisherExchange<VolumePublisher>(configuration);
            services.AddPublisherExchange<DryerPublisher>(configuration);
            return services;
        }
        public static IServiceCollection AddPublishers(this IServiceCollection services)
        {
            services.AddSingleton<ReactorPublisher>();
            services.AddSingleton<DecanterPublisher>();
            services.AddSingleton<NaOHTankPublisher>();
            services.AddSingleton<EthanolPublisher>();
            services.AddSingleton<OilTankPublisher>();
            services.AddSingleton<WashPublisher>();
            services.AddSingleton<VolumePublisher>();
            services.AddSingleton<DryerPublisher>();
            return services;
        }

        public static IServiceCollection AddConsumersQueues(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConsumerQueue<ReactorConsumer>(configuration);
            services.AddConsumerQueue<DecanterConsumer>(configuration);
            services.AddConsumerQueue<WashConsumer>(configuration);
            services.AddConsumerQueue<GlycerinConsumer>(configuration);
            services.AddConsumerQueue<TransactionConsumer>(configuration);
            services.AddConsumerQueue<DryerConsumer>(configuration);
            services.AddConsumerQueue<BiodiselConsumer>(configuration);
            return services;
        }

        public static IServiceCollection AddConsumers(this IServiceCollection services)
        {
            services.AddSingleton<ReactorConsumer>();
            services.AddSingleton<WashConsumer>();
            services.AddSingleton<DecanterConsumer>();
            services.AddSingleton<GlycerinConsumer>();
            services.AddSingleton<TransactionConsumer>();
            services.AddSingleton<DryerConsumer>();
            services.AddSingleton<BiodiselConsumer>();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IReactorService, ReactorService>();
            services.AddSingleton<Worker>();
            return services;
        }
        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<TransactionConsumer>();
            services.AddHostedService<ReactorConsumer>();
            services.AddHostedService<WashConsumer>();
            services.AddHostedService<DecanterConsumer>();
            services.AddHostedService<GlycerinConsumer>();
            services.AddHostedService<DryerConsumer>();
            services.AddHostedService<BiodiselConsumer>();
            return services;
        }

        public static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddSingleton<IQueueFactory, QueueFactory>();
            return services;
        }
    }
}