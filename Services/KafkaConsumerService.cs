using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Kafka Consumer chạy background, đọc từ topic raw_events.
    /// Khi có event mới → lưu vào bộ nhớ + broadcast tới tất cả SSE clients.
    /// </summary>
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumerService> _logger;

        // Lưu events trong bộ nhớ
        private static readonly ConcurrentQueue<NormalizedEvent> _events = new();

        // Channel để broadcast tới SSE clients
        private static readonly ConcurrentDictionary<string, Channel<NormalizedEvent>> _clients = new();

        public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "webhook-ui-consumer",
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = true
            };

            // Hỗ trợ Confluent Cloud SASL authentication
            var kafkaUsername = _configuration["Kafka:Username"];
            var kafkaPassword = _configuration["Kafka:Password"];
            if (!string.IsNullOrEmpty(kafkaUsername) && !string.IsNullOrEmpty(kafkaPassword))
            {
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.ScramSha256;
                config.SaslUsername = kafkaUsername;
                config.SaslPassword = kafkaPassword;
                config.SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
            }

            var topic = _configuration["Kafka:Topic"] ?? "raw_events";

            _logger.LogInformation("Kafka Consumer STARTED | Topic: {Topic}", topic);

            await Task.Run(() =>
            {
                using var consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Reason}", e.Reason))
                    .Build();

                consumer.Subscribe(topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                        if (result == null) continue;

                        var evt = JsonSerializer.Deserialize<NormalizedEvent>(result.Message.Value);
                        if (evt == null) continue;

                        // Lưu vào bộ nhớ
                        _events.Enqueue(evt);

                        _logger.LogInformation(
                            "Kafka >>> UI | Type: {Type} | From: {From} | Content: \"{Msg}\"",
                            evt.EventType, evt.Sender?.Name ?? "?",
                            evt.Content?.Length > 40 ? evt.Content[..40] + "..." : evt.Content);

                        // Broadcast tới tất cả SSE clients
                        foreach (var client in _clients)
                        {
                            client.Value.Writer.TryWrite(evt);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError("Kafka consume error: {Msg}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Consumer error: {Msg}", ex.Message);
                    }
                }

                consumer.Close();
            }, stoppingToken);
        }

        /// <summary>Lấy tất cả events đã lưu</summary>
        public static List<NormalizedEvent> GetAllEvents()
        {
            return _events.ToList();
        }

        /// <summary>Đăng ký SSE client mới</summary>
        public static Channel<NormalizedEvent> Subscribe(string clientId)
        {
            var channel = Channel.CreateUnbounded<NormalizedEvent>();
            _clients.TryAdd(clientId, channel);
            return channel;
        }

        /// <summary>Hủy đăng ký SSE client</summary>
        public static void Unsubscribe(string clientId)
        {
            _clients.TryRemove(clientId, out _);
        }
    }
}
