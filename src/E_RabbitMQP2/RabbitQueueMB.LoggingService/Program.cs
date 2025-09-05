using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitQueueMB.LoggingService;

internal class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "payments_log", durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"LOGGING: {message}");
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync(queue: "logging.queue", autoAck: false, consumer: consumer);

        Console.WriteLine("Logging Service started. Press Ctrl+C to exit.");
        while (true)
        {
            await Task.Delay(1000);
        }
    }
}