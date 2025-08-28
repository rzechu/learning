using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitQueueMB.WebApi.Services;

public class RabbitMqPublisher : IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly string _hostname;
    private readonly string _exchangeName = "payments_direct";
    private IConnection _connection;
    private IChannel _channel;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _hostname = configuration["RabbitMQ:Host"] ?? throw new ArgumentNullException("RabbitMQ:Host is not configured.");
        ConnectToRabbitMqAsync();
    }

    private async Task ConnectToRabbitMqAsync()
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare the direct exchange
            await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, durable: true);
            _logger.LogInformation("Connected to RabbitMQ and declared exchange '{ExchangeName}'", _exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ at {Hostname}", _hostname);
        }
    }

    public async Task PublishPaymentIdAsync(Guid paymentId, string paymentMethod)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("RabbitMQ channel is not open. Attempting to reconnect...");
            await ConnectToRabbitMqAsync();
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogError("Failed to reconnect to RabbitMQ. Message will not be published.");
                return;
            }
        }

        try
        {
            var routingKey = paymentMethod.ToLower() switch
            {
                "creditcard" => "payments_card",
                "paypal" => "payments_paypal",
                _ => throw new ArgumentException("Unknown payment method")
            };

            var message = JsonSerializer.Serialize(new { PaymentId = paymentId });
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = DeliveryModes.Persistent;

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body
            );
            _logger.LogInformation("Published payment ID '{PaymentId}' to RabbitMQ with routing key '{RoutingKey}'.", paymentId, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ for payment ID '{PaymentId}'.", paymentId);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}