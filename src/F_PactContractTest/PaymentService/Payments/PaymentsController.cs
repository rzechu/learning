using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Payments;

/// <summary>
/// Payments
/// </summary>
[ApiController]
[Route("/api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _payments;

    /// <summary>
    /// Initialises a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="paymentsRepository">Payments repository</param>
    public PaymentsController(IPaymentRepository paymentsRepository)
    {
        _payments = paymentsRepository;
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment</returns>
    /// <response code="200">Payment</response>
    /// <response code="404">Unknown payment</response>
    [HttpGet("{id}", Name = "get")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            PaymentDto payment = await _payments.GetAsync(id);
            return Ok(payment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <returns>Created payment</returns>
    /// <response code="201">Created payment</response>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(PaymentDto payment)
    {
        int id = new Random().Next();
        var paymentDto = new PaymentDto(id, payment.Amount, payment.Currency, payment.Method, payment.CardNumber);

        await _payments.InsertAsync(paymentDto);
        return CreatedAtRoute("get", new { id = payment.PaymentId }, paymentDto);
    }
}