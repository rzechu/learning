using System.Threading.Tasks;

namespace CheckoutService;

public interface IPaymentsClient
{
    /// <summary>
    /// Get an payment by ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment</returns>
    Task<PaymentDto> GetPaymentAsync(int paymentId);

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="paymentDto"></param>
    /// <returns>Payment</returns>
    Task<PaymentDto> CreatePaymentAsync(PaymentDto paymentDto);
}
