using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sharding.WebApi.Models;

public class Clinic
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; } // MongoDB's _id
    public string ClinicId { get; set; } // Shard key
    public string Name { get; set; }
    public string Location { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}