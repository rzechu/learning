using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitQueueMB.AnalyticsService;

internal class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "analytics.queue", durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"ANALYTICS: {message}");
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync(queue: "analytics.queue", autoAck: false, consumer: consumer);

        Console.WriteLine("Analytics Service started. Press [enter] to exit.");
        Console.ReadLine();
    }
}