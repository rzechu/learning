using System.Threading.Tasks;

namespace CheckoutService;

/// <summary>
/// Subscribes to events on the Payments API
/// </summary>
public class PaymentCreatedConsumer
{
    private readonly IFulfilmentService _fulfilment;

    /// <summary>
    /// Initialises a new instance of the <see cref="PaymentCreatedConsumer"/> class.
    /// </summary>
    /// <param name="fulfilment">Fulfilment service</param>
    public PaymentCreatedConsumer(IFulfilmentService fulfilment)
    {
        _fulfilment = fulfilment;
    }

    /// <summary>
    /// Process an event which identifies that an payment has been created
    /// </summary>
    /// <param name="message">Payment created event</param>
    /// <returns>Awaitable</returns>
    public async ValueTask OnMessageAsync(PaymentCreatedEvent message)
    {
        await _fulfilment.FulfilPaymentAsync(message.Id);
    }
}