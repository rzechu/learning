using System.Threading.Tasks;

namespace CheckoutService;

/// <summary>
/// Service for fulfilling payments
/// </summary>
public interface IFulfilmentService
{
    /// <summary>
    /// Fulfil the given payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Awaitable</returns>
    Task<PaymentDto> FulfilPaymentAsync(int paymentId);
}
