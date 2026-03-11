using Confluent.Kafka;
using EmailWorkerService.Messages;
using EmailWorkerService.Services;
using Prometheus;
using System.Text.Json;

namespace EmailWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly string _topicName;

        private static readonly Counter MessagesConsumed = Metrics.CreateCounter(
            "email_worker_messages_consumed_total",
            "Total number of Kafka messages consumed by the email worker");

        private static readonly Counter MessagesProcessed = Metrics.CreateCounter(
            "email_worker_messages_processed_total",
            "Total number of messages successfully processed (emails sent)");

        private static readonly Counter MessagesErrored = Metrics.CreateCounter(
            "email_worker_messages_errored_total",
            "Total number of messages that failed processing");

        private static readonly Histogram ProcessingDuration = Metrics.CreateHistogram(
            "email_worker_processing_duration_seconds",
            "Histogram of message processing duration in seconds");

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IEmailSender emailSender)
        {
            _logger = logger;
            _configuration = configuration;
            _emailSender = emailSender;
            _topicName = configuration["Kafka:Topics:EmailNotifications"] ?? "email-notifications";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "email-notification-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_topicName);

            _logger.LogInformation("Email Worker started. Listening for messages on topic: {Topic}", _topicName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value != null)
                    {
                        MessagesConsumed.Inc();
                        _logger.LogInformation("Received message: {Message}", consumeResult.Message.Value);

                        using (ProcessingDuration.NewTimer())
                        {
                            var emailMessage = JsonSerializer.Deserialize<EmailNotificationMessage>(
                                consumeResult.Message.Value,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (emailMessage != null)
                            {
                                await _emailSender.SendEmailAsync(emailMessage);
                                MessagesProcessed.Inc();
                            }
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    MessagesErrored.Inc();
                    _logger.LogError(ex, "Error consuming message");
                }
                catch (Exception ex)
                {
                    MessagesErrored.Inc();
                    _logger.LogError(ex, "Error processing message");
                }
            }

            consumer.Close();
        }
    }
}