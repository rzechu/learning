using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using PactNet;
using PactNet.Output.Xunit;
using Xunit;
using Xunit.Abstractions;
using Match = PactNet.Matchers.Match;

namespace CheckoutService.Tests;

public class PaymentCreatedConsumerTests
{
    private readonly PaymentCreatedConsumer _consumer;
    private readonly Mock<IFulfilmentService> _mockService;

    private readonly IMessagePactBuilderV4 _pact;

    public PaymentCreatedConsumerTests(ITestOutputHelper output)
    {
        _mockService = new Mock<IFulfilmentService>();
        _consumer = new PaymentCreatedConsumer(_mockService.Object);

        var config = new PactConfig
        {
            PactDir = "../../../pacts/",
            Outputters = new[]
            {
                new XunitOutput(output)
            },
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            }
        };

        _pact = Pact.V4("Fulfilment API", "Payment API", config).WithMessageInteractions();
    }

    [Fact]
    public async Task OnMessageAsync_PaymentCreated_HandlesMessage()
    {
        await _pact
                  .ExpectsToReceive("an event indicating that an payment has been created")
                  .WithJsonContent(new
                  {
                      Id = Match.Integer(1)
                  })
                  .VerifyAsync<PaymentCreatedEvent>(async message =>
                  {
                      await _consumer.OnMessageAsync(message);

                      _mockService.Verify(s => s.FulfilPaymentAsync(message.Id));
                  });
    }
}