using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sharding.WebApi.Models;

public class Patient
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ClinicId { get; set; } // Shard key
    public string PatientId { get; set; }
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public DateTime RegisteredAt { get; set; }
}