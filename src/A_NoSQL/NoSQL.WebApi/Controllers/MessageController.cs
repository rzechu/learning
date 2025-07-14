using Microsoft.AspNetCore.Mvc;
using NoSQL.WebApi.Models;
using NoSQL.WebApi.Repositories;

namespace NoSQL.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IMessageRepository messageRepository,
        ILogger<MessagesController> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Message>> SendMessage([FromBody] Message message)
    {
        if (message.SenderId == message.ReceiverId)
            return BadRequest("Sender and receiver cannot be the same");

        if (string.IsNullOrWhiteSpace(message.Content))
            return BadRequest("Message content cannot be empty");

        message.Id = Guid.NewGuid();
        message.Timestamp = DateTime.UtcNow;
        message.IsRead = false;

        var sentMessage = await _messageRepository.SendMessageAsync(message);
        return Created($"/api/messages/{message.SenderId}/{message.ReceiverId}", sentMessage);
    }

    [HttpGet("{user1Id}/{user2Id}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(
        Guid user1Id,
        Guid user2Id,
        [FromQuery] int limit = 100)
    {
        var messages = await _messageRepository.GetMessagesAsync(user1Id, user2Id, limit);
        return Ok(messages);
    }
}