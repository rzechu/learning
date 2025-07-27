using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sharding.WebApi.Services;
using Sharding.WebApi.Settings;

namespace Sharding.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ehr_db";

        builder.Services.AddSingleton(new MongoDBService(mongoDbConnectionString, databaseName));


        builder.Services.AddScoped<MongoDBService>(); 

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        //app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}