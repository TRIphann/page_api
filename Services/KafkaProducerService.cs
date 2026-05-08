using System.Text.Json;
using Confluent.Kafka;
using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Service publish NormalizedEvent vào Kafka topic raw_events.
    /// Sử dụng Confluent.Kafka producer.
    /// </summary>
    public interface IKafkaProducerService : IDisposable
    {
        Task<bool> PublishEventAsync(NormalizedEvent normalizedEvent);
    }

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _topic = configuration["Kafka:Topic"] ?? "raw_events";

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                ClientId = "facbook-page-api",
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                LingerMs = 5,
                BatchSize = 16384
            };

            // Hỗ trợ Confluent Cloud SASL authentication
            var kafkaUsername = configuration["Kafka:Username"];
            var kafkaPassword = configuration["Kafka:Password"];
            if (!string.IsNullOrEmpty(kafkaUsername) && !string.IsNullOrEmpty(kafkaPassword))
            {
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.ScramSha256;
                config.SaslUsername = kafkaUsername;
                config.SaslPassword = kafkaPassword;
                config.SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
            }

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError("Kafka producer error: {Reason}", error.Reason);
                })
                .Build();

            _logger.LogInformation("KafkaProducer initialized → Topic: {Topic}, Servers: {Servers}",
                _topic, config.BootstrapServers);
        }

        public async Task<bool> PublishEventAsync(NormalizedEvent normalizedEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(normalizedEvent);

                var message = new Message<string, string>
                {
                    Key = normalizedEvent.PageId,
                    Value = json
                };

                var result = await _producer.ProduceAsync(_topic, message);

                _logger.LogInformation(
                    "✅ Event → Kafka | Topic: {Topic} | Partition: {Partition} | Offset: {Offset} | Type: {Type} | ID: {Id}",
                    result.Topic, result.Partition.Value, result.Offset.Value,
                    normalizedEvent.EventType, normalizedEvent.EventId);

                return true;
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Kafka publish failed | Type: {Type} | Error: {Error}",
                    normalizedEvent.EventType, ex.Error.Reason);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected Kafka error | EventId: {Id}", normalizedEvent.EventId);
                return false;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
        }
    }
}
