using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sharding.WebApi.Models;

public class Visit
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    public string ClinicId { get; set; } // Shard key
    public string PatientId { get; set; }
    public DateTime VisitDate { get; set; }
    public string Doctor { get; set; }
    public string Diagnosis { get; set; }
    public List<string> Prescriptions { get; set; }
    public string Notes { get; set; }
}