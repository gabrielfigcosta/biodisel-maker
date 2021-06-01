namespace biodisel.domain.Interfaces
{
    public interface IQueueFactory
    {
        void CreateDefaultAndBind(string queueName, string routingKey, string exchange);
    }
}