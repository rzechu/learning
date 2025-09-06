using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.PubSub;

namespace PaymentService.Payments;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMessagePublisher _publisher;

    // NOTE: for this demo this uses an in-memory store but in reality this would store to a database or something
    private readonly ConcurrentDictionary<int, PaymentDto> _payments = new();

    /// <summary>
    /// Initialises a new instance of the <see cref="PaymentRepository"/> class.
    /// </summary>
    /// <param name="publisher">Message publisher</param>
    public PaymentRepository(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

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
    public async Task InsertAsync(PaymentDto payment)
    {
        _payments[payment.PaymentId] = payment;

        // notify subscribers of the new payment
        await this._publisher.PublishAsync(new PaymentCreatedEvent(payment.PaymentId));
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