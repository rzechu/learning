using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.WebApi.Models;

public class Order
{
    [BsonId] // Marks this property as the primary key for MongoDB
    [BsonRepresentation(BsonType.String)] // Stores the Guid as a string in MongoDB
    public Guid Id { get; set; } = Guid.NewGuid(); // Auto-generate GUID for new orders

    public string CustomerId { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    public decimal TotalAmount { get; set; }

    // Status: "Pending", "Paid", "Cancelled"
    public string Status { get; set; } = "Pending";

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}