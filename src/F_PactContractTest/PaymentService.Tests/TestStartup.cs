using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Payments;

namespace PaymentService.Tests;

public class TestStartup
{
    private readonly Startup _inner;

    public TestStartup(IConfiguration configuration)
    {
        _inner = new Startup(configuration);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPaymentRepository, FakePaymentRepository>();

        _inner.ConfigureServices(services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ProviderStateMiddleware>();

        _inner.Configure(app, env);
    }
}