using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Payments;

namespace PaymentService.Tests;

/// <summary>
/// Fake for <see cref="IPaymentRepository"/>
/// </summary>zd
public class FakePaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<int, PaymentDto> _payments = new();

    /// <summary>
    /// Get an payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment</returns>
    /// <exception cref="KeyNotFoundException">Payment with the given ID was not found</exception>
    public Task<PaymentDto> GetAsync(int id)
    {
        PaymentDto payment = _payments[id];
        return Task.FromResult(payment);
    }

    /// <summary>
    /// Insert an payment
    /// </summary>
    /// <param name="payment">Payment to insert</param>
    /// <returns>Awaitable</returns>
    public Task InsertAsync(PaymentDto payment)
    {
        _payments[payment.PaymentId] = payment;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Update an payment
    /// </summary>
    /// <param name="payment">Payment with updated state</param>
    /// <returns>Awaitable</returns>
    public Task UpdateAsync(PaymentDto payment)
    {
        _payments[payment.PaymentId] = payment;
        return Task.CompletedTask;
    }
}
