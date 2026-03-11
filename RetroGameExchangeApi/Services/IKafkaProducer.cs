namespace RetroGameExchangeApi.Services
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string message);
    }
}
