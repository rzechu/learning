using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sharding.WebApi.Models;
using Sharding.WebApi.Settings;

namespace Sharding.WebApi.BackgroundServices;

public class MongoDbInitializer : IHostedService
{
    private readonly ILogger<MongoDbInitializer> _logger;
    private readonly IMongoClient _mongoClient;
    private readonly MongoDBSettings _mongoDbSettings;

    public MongoDbInitializer(ILogger<MongoDbInitializer> logger, IMongoClient mongoClient, IOptions<MongoDBSettings> mongoDbSettings)
    {
        _logger = logger;
        _mongoClient = mongoClient;
        _mongoDbSettings = mongoDbSettings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MongoDB Initializer starting (ensuring collections/indexes)...");
        await EnsureCollectionExistsAndIndexes(cancellationToken);
    }

    private async Task EnsureCollectionExistsAndIndexes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring collection and indexes are set up...");
        try
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);

            var clinicsCollection = database.GetCollection<Clinic>("Clinics");
            await clinicsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Clinic>(
                    Builders<Clinic>.IndexKeys.Ascending(c => c.ClinicId),
                    new CreateIndexOptions { Unique = true, Name = "ClinicId_idx" }));

            var patientsCollection = database.GetCollection<Patient>("Patients");
            await patientsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Patient>(
                    Builders<Patient>.IndexKeys.Combine(
                        Builders<Patient>.IndexKeys.Ascending(p => p.ClinicId),
                        Builders<Patient>.IndexKeys.Ascending(p => p.PatientId)),
                    new CreateIndexOptions { Unique = true, Name = "Clinic_Patient_idx" }));

            var visitsCollection = database.GetCollection<Visit>("Visits");
            await visitsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Visit>(
                    Builders<Visit>.IndexKeys.Combine(
                        Builders<Visit>.IndexKeys.Ascending(v => v.ClinicId),
                        Builders<Visit>.IndexKeys.Ascending(v => v.PatientId),
                        Builders<Visit>.IndexKeys.Descending(v => v.VisitDate)),
                    new CreateIndexOptions { Name = "VisitSearch_idx" }));

            _logger.LogInformation("MongoDB collection and index setup finished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MongoDB collection/index setup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MongoDB Initializer stopping.");
        return Task.CompletedTask;
    }
}