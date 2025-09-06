namespace PaymentService.Payments;

/// <summary>
/// An order has been created
/// </summary>
/// <param name="Id">ID of the created order</param>
public record PaymentCreatedEvent(int Id);
