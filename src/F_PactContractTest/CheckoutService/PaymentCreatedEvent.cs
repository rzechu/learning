namespace CheckoutService;

/// <summary>
/// An payment has been created
/// </summary>
/// <param name="Id">ID of the created payment</param>
public record PaymentCreatedEvent(int Id);