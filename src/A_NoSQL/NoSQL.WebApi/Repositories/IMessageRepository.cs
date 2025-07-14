using NoSQL.WebApi.Models;

namespace NoSQL.WebApi.Repositories;

public interface IMessageRepository
{
    Task<Message> SendMessageAsync(Message message);
    Task<IEnumerable<Message>> GetMessagesAsync(Guid user1Id, Guid user2Id, int limit = 100);
}