using System.Threading.Tasks;

namespace CheckoutService;

/// <summary>
/// Service for fulfilling payments
/// </summary>
public class FulfilmentService : IFulfilmentService
{
    private readonly IPaymentsClient _client;

    /// <summary>
    /// Initialises a new instance of the <see cref="FulfilmentService"/> class.
    /// </summary>
    /// <param name="client">Payments client</param>
    public FulfilmentService(IPaymentsClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Fulfil the given payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Awaitable</returns>
    public async Task<PaymentDto> FulfilPaymentAsync(int paymentId)
    {
        var payment = await _client.GetPaymentAsync(paymentId);
        return payment;
    }
}
