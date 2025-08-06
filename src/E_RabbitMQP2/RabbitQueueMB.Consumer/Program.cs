
using Microsoft.EntityFrameworkCore;
using RabbitQueueMB.Consumer.Data;

namespace RabbitQueueMB.Consumer;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Configure PostgreSQL connection
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString),
                    ServiceLifetime.Scoped // DbContext should be Scoped for BackgroundService
                );

                // Register the Worker as a hosted service
                services.AddHostedService<Worker>();
            });
}