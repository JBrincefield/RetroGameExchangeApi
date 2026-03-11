using Confluent.Kafka;

namespace RetroGameExchangeApi.Services
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _logger = logger;

            var bootstrapServers = configuration["Kafka:BootstrapServers"]
                ?? Environment.GetEnvironmentVariable("Kafka__BootstrapServers")
                ?? "localhost:9092";

            _logger.LogInformation("Kafka BootstrapServers: {BootstrapServers}", bootstrapServers);

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, string message)
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            _logger.LogInformation("Produced message to topic {Topic}, Partition: {Partition}, Offset: {Offset}",
                topic, result.Partition.Value, result.Offset.Value);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
