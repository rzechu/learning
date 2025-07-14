using Cassandra;

namespace NoSQL.WebApi.Services;

public class CassandraMigrationService : ICassandraMigrationService
{
    private readonly Cassandra.ISession _session;
    private readonly ILogger<CassandraMigrationService> _logger;

    public CassandraMigrationService(
        Cassandra.ISession session,
        ILogger<CassandraMigrationService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task InitializeDatabaseSchemaAsync()
    {
        try
        {
            await _session.ExecuteAsync(new SimpleStatement(
                @"CREATE KEYSPACE IF NOT EXISTS messaging_app 
                    WITH replication = {
                        'class': 'SimpleStrategy', 
                        'replication_factor': 3
                    }"
            ));

            await _session.ExecuteAsync(new SimpleStatement(
                @"CREATE TABLE IF NOT EXISTS messaging_app.messages_by_conversation (
                        user1_id UUID,
                        user2_id UUID,
                        message_id UUID,
                        sender_id UUID,
                        receiver_id UUID,
                        content TEXT,
                        timestamp TIMESTAMP,
                        is_read BOOLEAN,
                        PRIMARY KEY ((user1_id, user2_id), timestamp, message_id)
                    ) WITH CLUSTERING ORDER BY (timestamp DESC, message_id DESC)"
            ));

            _logger.LogInformation("Cassandra database schema initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Cassandra database schema");
            throw;
        }
    }
}