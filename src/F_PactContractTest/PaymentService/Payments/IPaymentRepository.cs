using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentService.Payments;

/// <summary>
/// Repository for managing payment instances
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Get an payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment</returns>
    /// <exception cref="KeyNotFoundException">Payment with the given ID was not found</exception>
    Task<PaymentDto> GetAsync(int id);

    /// <summary>
    /// Insert an payment
    /// </summary>
    /// <param name="payment">Payment to insert</param>
    /// <returns>Awaitable</returns>
    Task InsertAsync(PaymentDto payment);

    /// <summary>
    /// Update an payment
    /// </summary>
    /// <param name="payment">Payment with updated state</param>
    /// <returns>Awaitable</returns>
    Task UpdateAsync(PaymentDto payment);
}
