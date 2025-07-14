using Cassandra;
using NoSQL.WebApi.Models;

namespace NoSQL.WebApi.Repositories;

public class CassandraMessageRepository : IMessageRepository
{
    private readonly Cassandra.ISession _session;
    private readonly ILogger<CassandraMessageRepository> _logger;
    private PreparedStatement _insertStatement;
    private PreparedStatement _selectStatement;

    public CassandraMessageRepository(
        Cassandra.ISession session,
        ILogger<CassandraMessageRepository> logger)
    {
        _session = session;
        _logger = logger;
        InitializePreparedStatements();
    }

    private void InitializePreparedStatements()
    {
        _insertStatement = _session.Prepare(
            @"INSERT INTO messaging_app.messages_by_conversation 
                (user1_id, user2_id, message_id, sender_id, receiver_id, content, timestamp, is_read) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)"
        );

        _selectStatement = _session.Prepare(
            @"SELECT message_id, sender_id, receiver_id, content, timestamp, is_read 
                FROM messaging_app.messages_by_conversation 
                WHERE user1_id = ? AND user2_id = ? 
                LIMIT ?"
        );
    }

    private (Guid user1Id, Guid user2Id) NormalizeUserIds(Guid userId1, Guid userId2)
    {
        // Always return in consistent order for partition key
        return userId1.CompareTo(userId2) < 0
            ? (userId1, userId2)
            : (userId2, userId1);
    }
    public async Task<Message> SendMessageAsync(Message message)
    {
        try
        {
            var (user1Id, user2Id) = NormalizeUserIds(message.SenderId, message.ReceiverId);

            var boundStatement = _insertStatement.Bind(
                user1Id, user2Id,
                message.Id, message.SenderId, message.ReceiverId,
                message.Content, message.Timestamp, message.IsRead
            );

            boundStatement.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);
            await _session.ExecuteAsync(boundStatement);

            _logger.LogInformation("Message sent: {MessageId}", message.Id);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(Guid user1Id, Guid user2Id, int limit = 100)
    {
        try
        {
            var (normalizedUser1Id, normalizedUser2Id) = NormalizeUserIds(user1Id, user2Id);

            var boundStatement = _selectStatement.Bind(normalizedUser1Id, normalizedUser2Id, limit);
            boundStatement.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);

            var rowSet = await _session.ExecuteAsync(boundStatement);

            var messages = new List<Message>();
            foreach (var row in rowSet)
            {
                messages.Add(new Message
                {
                    Id = row.GetValue<Guid>("message_id"),
                    SenderId = row.GetValue<Guid>("sender_id"),
                    ReceiverId = row.GetValue<Guid>("receiver_id"),
                    Content = row.GetValue<string>("content"),
                    Timestamp = row.GetValue<DateTime>("timestamp"),
                    IsRead = row.GetValue<bool>("is_read")
                });
            }

            return messages.OrderByDescending(m => m.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages");
            throw;
        }
    }
}