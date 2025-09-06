using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PactNet;
using PactNet.Output.Xunit;
using Xunit;
using Xunit.Abstractions;
using Match = PactNet.Matchers.Match;

namespace CheckoutService.Tests;

public class PaymentsClientTests
{
    private readonly IPactBuilderV4 _pact;
    private readonly Mock<IHttpClientFactory> _mockFactory;

    public PaymentsClientTests(ITestOutputHelper output)
    {
        _mockFactory = new Mock<IHttpClientFactory>();

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
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            },
            LogLevel = PactLogLevel.Debug
        };

        _pact = Pact.V4("Fulfilment API", "Payments API", config).WithHttpInteractions();
    }

    [Fact]
    public async Task GetPaymentAsync_WhenCalled_ReturnsPayment()
    {
        var expected = new PaymentDto(1, 123, "USD", "CreditCard", "123 33 009");

        _pact
            .UponReceiving("a request for an payment by ID")
                .Given("an payment with ID {id} exists", new Dictionary<string, string> { ["id"] = "1" })
                .WithRequest(HttpMethod.Get, "/api/payments/1")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithJsonBody(new
                {
                    PaymentId = Match.Integer(expected.PaymentId),// PactNet.Matchers.IntegerMatcher(),
                    Amount = Match.Decimal(expected.Amount),
                    Currency = Match.Regex(expected.Currency, "^[A-Z]{3}$"),
                    Method = "CreditCard", //Match.Type(typeof(string)),
                    CardNumber = "123 33 009" //Match.Type(typeof(string))
                });

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Payments"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders =
                    {
                        Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                    }
                });

            var client = new PaymentsClient(_mockFactory.Object);

            PaymentDto payment = await client.GetPaymentAsync(1);

            payment.Should().Be(expected);
        });
    }

    [Fact]
    public async Task GetPaymentAsync_UnknownPayment_ReturnsNotFound()
    {
        _pact
            .UponReceiving("a request for an payment with an unknown ID")
                .WithRequest(HttpMethod.Get, "/api/payments/404")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound);

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Payments"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders =
                    {
                        Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                    }
                });

            var client = new PaymentsClient(_mockFactory.Object);

            Func<Task> action = () => client.GetPaymentAsync(404);

            var response = await action.Should().ThrowAsync<HttpRequestException>();
            response.And.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task CreatePaymentAsync_PaymentCreated()
    {
        _pact
            .UponReceiving("a request for an payment with an unknown ID")
                .WithRequest(HttpMethod.Post, "/api/payments")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.Created);

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Payments"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders =
                    {
                        Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                    }
                });

            var client = new PaymentsClient(_mockFactory.Object);

            PaymentDto payment = new PaymentDto(0, 123, "USD", "CreditCard", "123 33 009");
            Func<Task> action = () => client.CreatePaymentAsync(payment);

            var response = await action.Should().NotThrowAsync<HttpRequestException>();
            response.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task CreatePaymentAsync_BadRequest()
    {
        _pact
            .UponReceiving("a request for an payment with bad values ")
                .WithRequest(HttpMethod.Post, "/api/payments")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.BadRequest);

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Payments"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders =
                    {
                        Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                    }
                });

            var client = new PaymentsClient(_mockFactory.Object);

            Func<Task> action = () => client.CreatePaymentAsync(null);

            var response = await action.Should().ThrowAsync<HttpRequestException>();
            response.And.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }
}