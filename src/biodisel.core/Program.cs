using biodisel.core.Extensions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Core.Extensions;

namespace biodisel.core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // services.AddMQConfigBindindgs(hostContext.Configuration);
                    // services.AddMQExchanges();
                    // services.AddConsumersQueues(hostContext.Configuration);
                    // services.AddPublishersExchanges(hostContext.Configuration);
                    // services.AddConsumers();
                    // services.AddPublishers();
                    // services.AddApplicationServices();
                    // services.AddFactories();
                    // services.AddHostedServices();
                });
    }
}
