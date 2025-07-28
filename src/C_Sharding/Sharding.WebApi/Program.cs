using Sharding.WebApi.Services;

namespace Sharding.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Get MongoDB connection details, prioritizing environment variables
        var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "mongos1";
        var mongoPort = Environment.GetEnvironmentVariable("MONGODB_PORT") ?? "27017";
        var mongoDbConnectionString = $"mongodb://{mongoHost}:{mongoPort}";
        var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ehr_db";

        // Register as scoped service with proper connection string
        builder.Services.AddScoped<MongoDBService>(sp =>
            new MongoDBService(mongoDbConnectionString, databaseName));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}