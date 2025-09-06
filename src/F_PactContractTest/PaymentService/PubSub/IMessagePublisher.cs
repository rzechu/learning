using System.Threading.Tasks;

namespace PaymentService.PubSub;

/// <summary>
/// Service for publishing messages
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a message
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <returns>Awaitable</returns>
    ValueTask PublishAsync<T>(T message);
}
