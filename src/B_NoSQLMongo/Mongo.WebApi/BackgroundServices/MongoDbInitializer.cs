using Microsoft.Extensions.Options;
using Mongo.WebApi.Models;
using Mongo.WebApi.Settings;
using MongoDB.Driver;

namespace Mongo.WebApi.BackgroundServices;

public class MongoDbInitializer : IHostedService
{
    private readonly ILogger<MongoDbInitializer> _logger;
    private readonly IMongoClient _mongoClient;
    private readonly MongoDbSettings _mongoDbSettings;

    public MongoDbInitializer(ILogger<MongoDbInitializer> logger, IMongoClient mongoClient, IOptions<MongoDbSettings> mongoDbSettings)
    {
        _logger = logger;
        _mongoClient = mongoClient;
        _mongoDbSettings = mongoDbSettings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MongoDB Initializer starting (ensuring collections/indexes)...");

        // Removed the call to EnsureReplicaSetInitiated as it's now handled by docker-compose's init-mongo service
        await EnsureCollectionExistsAndIndexes(cancellationToken);
    }

    // Removed the private async Task EnsureReplicaSetInitiated(...) method

    private async Task EnsureCollectionExistsAndIndexes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring collection and indexes are set up...");
        try
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);

            var collection = database.GetCollection<object>(_mongoDbSettings.CollectionName); // Using object for generic checks

            // Check if collection exists (MongoDB implicitly creates on first write, but this verifies connectivity)
            var collections = await database.ListCollectionNames().ToListAsync(cancellationToken);
            if (!collections.Contains(_mongoDbSettings.CollectionName))
            {
                _logger.LogInformation($"Collection '{_mongoDbSettings.CollectionName}' does not exist yet. It will be created on first data insertion.");
                // You could optionally create it explicitly if you prefer:
                // await database.CreateCollectionAsync(_mongoDbSettings.CollectionName, cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogInformation($"Collection '{_mongoDbSettings.CollectionName}' already exists.");
            }

            // Example: Create an index on CustomerId for efficient retrieval
            _logger.LogInformation($"Creating index on CustomerId for '{_mongoDbSettings.CollectionName}' collection.");
            var orderCollection = database.GetCollection<Order>(_mongoDbSettings.CollectionName); // Use specific type for index creation
            var customerIdIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.CustomerId);
            // CORRECTED: CreateIndexModel only needs IndexKeysDefinition and CreateIndexOptions
            var customerIdIndexModel = new CreateIndexModel<Order>(customerIdIndexKeys, new CreateIndexOptions { Name = "CustomerId_idx" });
            await orderCollection.Indexes.CreateOneAsync(customerIdIndexModel, cancellationToken: cancellationToken);
            _logger.LogInformation("Index on CustomerId created/ensured.");

            // Example: Create an index on Status for efficient background job queries
            _logger.LogInformation($"Creating index on Status for '{_mongoDbSettings.CollectionName}' collection.");
            var statusIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.Status).Ascending(o => o.CreatedAt); // Composite index
            // CORRECTED: CreateIndexModel only needs IndexKeysDefinition and CreateIndexOptions
            var statusIndexModel = new CreateIndexModel<Order>(statusIndexKeys, new CreateIndexOptions { Name = "Status_CreatedAt_idx" });
            await orderCollection.Indexes.CreateOneAsync(statusIndexModel, cancellationToken: cancellationToken);
            _logger.LogInformation("Index on Status and CreatedAt created/ensured.");


            _logger.LogInformation("MongoDB collection and index setup finished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MongoDB collection/index setup.");
            // Potentially re-throw or handle critical errors
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MongoDB Initializer stopping.");
        return Task.CompletedTask;
    }
}