using System.ComponentModel.DataAnnotations;

namespace NoSQL.WebApi.Models;

public partial class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}