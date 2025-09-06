using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using PaymentService.Payments;
using Xunit;
using Xunit.Abstractions;

namespace PaymentService.Tests;

public class ProviderTests : IDisposable
{
    private static readonly Uri ProviderUri = new("http://localhost:5000");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IHost _server;
    private readonly PactVerifier _verifier;

    public ProviderTests(ITestOutputHelper output)
    {
        _server = Host.CreateDefaultBuilder()
                          .ConfigureWebHostDefaults(webBuilder =>
                          {
                              webBuilder.UseUrls(ProviderUri.ToString());
                              webBuilder.UseStartup<TestStartup>();
                          })
                          .Build();

        _server.Start();
        
        _verifier = new PactVerifier("Payments API", new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Debug,
            Outputters = new List<IOutput>
            {
                new XunitOutput(output)
            }
        });
    }

    public void Dispose()
    {
        _server.Dispose();
        _verifier.Dispose();
    }

    [Fact]
    public void Verify()
    {
        string pactPath = Path.Combine("..",
                                       "..",
                                       "..",
                                       "..",
                                       "CheckoutService.Tests",
                                       "pacts",
                                       "Fulfilment API-Payment API.json");

        _verifier
            .WithHttpEndpoint(ProviderUri)
            .WithMessages(scenarios =>
            {
                scenarios.Add("an event indicating that an payment has been created", () => new PaymentCreatedEvent(1));
            }, Options)
            .WithFileSource(new FileInfo(pactPath))
            .WithProviderStateUrl(new Uri(ProviderUri, "/provider-states"))
            .Verify();
    }
}
