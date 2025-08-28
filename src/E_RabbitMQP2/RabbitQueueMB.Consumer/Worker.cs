using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitQueueMB.Consumer.Data;
using System.Text;
using System.Text.Json;

namespace RabbitQueueMB.Consumer 
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IChannel _channel;
        private readonly string _queueName;
        private readonly string _hostname;

        private const string PaymentQueue = "payment_queue";
        private const string PaymentDLQ = "payment_dlq";
        private const string PaymentDLQExchange = "payment.dlq.exchange";
        private const string ResultExchange = "payment.result.exchange";
        private const string NotificationQueue = "notification.queue";
        private const string AlertQueue = "alert.queue";
        private const string AnalyticsQueue = "analytics.queue";

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            _hostname = _configuration["RabbitMQ:Host"] ?? throw new ArgumentNullException("RabbitMQ:Host is not configured.");
            _queueName = _configuration["RabbitMQ:QueueName"] ?? "payment_queue";
        }

        private async Task<bool> InitializeRabbitMqAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostname };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("Consumer connected to RabbitMQ and declared queue '{QueueName}'", PaymentQueue);

                await _channel.ExchangeDeclareAsync(PaymentDLQExchange, ExchangeType.Fanout, durable: true);
                await _channel.QueueDeclareAsync(PaymentDLQ, durable: true, exclusive: false, autoDelete: false);
                await _channel.QueueBindAsync(PaymentDLQ, PaymentDLQExchange, "");

                var paymentQueueArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", PaymentDLQExchange }
                };
                await _channel.QueueDeclareAsync(PaymentQueue, durable: true, exclusive: false, autoDelete: false, arguments: paymentQueueArgs);

                await _channel.ExchangeDeclareAsync(ResultExchange, ExchangeType.Topic, durable: true);
                await _channel.QueueDeclareAsync(NotificationQueue, durable: true, exclusive: false, autoDelete: false);
                await _channel.QueueDeclareAsync(AlertQueue, durable: true, exclusive: false, autoDelete: false);
                await _channel.QueueDeclareAsync(AnalyticsQueue, durable: true, exclusive: false, autoDelete: false);

                await _channel.QueueBindAsync(NotificationQueue, ResultExchange, "payment.success");
                await _channel.QueueBindAsync(NotificationQueue, ResultExchange, "payment.failed");
                await _channel.QueueBindAsync(AlertQueue, ResultExchange, "payment.failed");
                await _channel.QueueBindAsync(AnalyticsQueue, ResultExchange, "payment.*");

                await _channel.ExchangeDeclareAsync("payments_direct", ExchangeType.Direct, durable: true);
                await _channel.QueueBindAsync(PaymentQueue, "payments_direct", "payments_card");
                await _channel.QueueBindAsync(PaymentQueue, "payments_direct", "payments_paypal");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not initialize RabbitMQ connection for consumer at {Hostname}", _hostname);
                return false;
            }
        }

        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var initialized = await InitializeRabbitMqAsync();
            if (!initialized || _channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized. Exiting consumer.");
                return Task.CompletedTask;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", message);

                int retryCount = 0;
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryObj))
                {
                    retryCount = int.Parse(Encoding.UTF8.GetString((byte[])retryObj));
                }

                Guid paymentId;
                try
                {
                    var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, Guid>>(message);
                    if (parsedMessage != null && parsedMessage.TryGetValue("PaymentId", out var id))
                    {
                        paymentId = id;
                    }
                    else
                    {
                        _logger.LogError("Invalid message format: {Message}", message);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing message: {Message}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var random = new Random();

                    try
                    {
                        var payment = await dbContext.Payments.FindAsync(paymentId);

                        if (payment == null)
                        {
                            _logger.LogWarning("Payment with ID '{PaymentId}' not found in database. Message acknowledged.", paymentId);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }

                        // 30% success rate
                        payment.Status = random.Next(10) < 3 ? "Processed" : "Failed";
                        payment.ProcessedAt = DateTime.UtcNow;

                        dbContext.Payments.Update(payment);
                        await dbContext.SaveChangesAsync();

                        // Publish result to topic exchange
                        var resultProps = new BasicProperties();
                        resultProps.Persistent = true;
                        string routingKey = payment.Status == "Processed" ? "payment.success" : "payment.failed";
                        var resultMessage = JsonSerializer.Serialize(new { PaymentId = payment.Id, OrderId = payment.OrderId, Status = payment.Status });
                        var resultBody = Encoding.UTF8.GetBytes(resultMessage);

                        await _channel.BasicPublishAsync(
                            exchange: ResultExchange,
                            routingKey: routingKey,
                            mandatory: true,
                            basicProperties: resultProps,
                            body: resultBody
                        );

                        if (payment.Status == "Processed" || retryCount >= 2)
                        {
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        }
                        else
                        {
                            // Retry: requeue with incremented retry count
                            var props = new BasicProperties();
                            props.Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                            props.Headers["x-retry-count"] = (retryCount + 1).ToString();
                            props.Persistent = true;

                            await _channel.BasicPublishAsync(
                                exchange: "",
                                routingKey: _queueName,
                                basicProperties: props,
                                mandatory: true,
                                body: ea.Body
                            );
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message for payment '{PaymentId}'.", paymentId);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName,
                                 autoAck: false, // Important: We manually acknowledge
                                 consumer: consumer);

            _logger.LogInformation("Consumer started listening for messages on queue '{QueueName}'", _queueName);
            
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
            _logger.LogInformation("RabbitMQ Consumer connection and channel disposed.");
        }
    }
}