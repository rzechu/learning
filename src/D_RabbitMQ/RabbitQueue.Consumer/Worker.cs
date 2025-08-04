using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitQueue.Consumer.Data;
using System.Text;
using System.Text.Json;

namespace RabbitQueue.Consumer 
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

                await _channel.QueueDeclareAsync(queue: _queueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("Consumer connected to RabbitMQ and declared queue '{QueueName}'", _queueName);
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
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); // Negative acknowledge, don't requeue
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing message: {Message}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); // Don't requeue malformed messages
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
                            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false); // Acknowledge even if not found to remove from queue
                            return;
                        }

                        // Randomly set status to Processed or Failed
                        payment.Status = random.Next(2) == 0 ? "Processed" : "Failed";
                        payment.ProcessedAt = DateTime.UtcNow;

                        dbContext.Payments.Update(payment);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Payment '{PaymentId}' status updated to '{Status}'.", payment.Id, payment.Status);

                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false); // Acknowledge message after successful processing
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _logger.LogError(dbEx, "Database update error for payment '{PaymentId}'. Message NACKed.", paymentId);
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true); // NACK and requeue on DB error
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message for payment '{PaymentId}'. Message NACKed.", paymentId);
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true); // NACK and requeue on general error
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