using Microsoft.AspNetCore.Mvc;
using RabbitQueueMB   .WebApi.Data;
using RabbitQueueMB.WebApi.Models;
using RabbitQueueMB.WebApi.Services;

namespace RabbitQueueMB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(AppDbContext context, RabbitMqPublisher rabbitMqPublisher, ILogger<PaymentsController> logger)
    {
        _context = context;
        _rabbitMqPublisher = rabbitMqPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new payment and queues it for processing.
    /// </summary>
    /// <param name="paymentRequest">Payment details (OrderId, Amount, Currency, PaymentMethod)</param>
    /// <returns>A response indicating the payment has been queued.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] Payment paymentRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = paymentRequest.OrderId,
            Amount = paymentRequest.Amount,
            Currency = paymentRequest.Currency,
            PaymentMethod = paymentRequest.PaymentMethod,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            if (payment?.PaymentMethod.ToLower() is not ("creditcard" or "paypal"))
            {
                _logger.LogWarning("Invalid payment method '{PaymentMethod}' for payment '{PaymentId}'", payment.PaymentMethod, payment.Id);
                return BadRequest("Invalid payment method. Supported methods are 'CreditCard' and 'PayPal'.");
            }

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            _rabbitMqPublisher.PublishPaymentIdAsync(payment.Id, payment.PaymentMethod);

            _logger.LogInformation("Payment '{PaymentId}' created and queued for processing.", payment.Id);
            return Accepted(new { Message = "Payment has been queued for processing.", PaymentId = payment.Id, Status = payment.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or queuing payment.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the payment.");
        }
    }

    /// <summary>
    /// Retrieves the status of a specific payment.
    /// </summary>
    /// <param name="id">The ID of the payment.</param>
    /// <returns>The payment details if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentStatus(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            _logger.LogWarning("Payment with ID '{PaymentId}' not found.", id);
            return NotFound($"Payment with ID '{id}' not found.");
        }

        _logger.LogInformation("Retrieved status for payment '{PaymentId}': {Status}", id, payment.Status);
        return Ok(payment);
    }
}