using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitQueueMB.WebApi.Models;

public class Payment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Pending";

    public DateTime? ProcessedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}