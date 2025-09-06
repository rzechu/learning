namespace PaymentService.Payments;

/// <summary>
/// Payment DTO
/// </summary>
/// <param name="PaymentId">Payment ID</param>
/// <param name="Amount">Amount</param>
/// <param name="Currency">Currency</param>
/// <param name="Method">Method</param>
/// <param name="CardNumber">Card number</param>
public record PaymentDto(int PaymentId,
    decimal Amount,
    string Currency,
    string Method,
    string CardNumber);
