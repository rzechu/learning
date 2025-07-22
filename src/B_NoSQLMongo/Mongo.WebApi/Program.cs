using Microsoft.Extensions.Options;
using Mongo.WebApi.BackgroundServices;
using Mongo.WebApi.Services;
using Mongo.WebApi.Settings;
using MongoDB.Driver;

namespace Mongo.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<MongoDbSettings>( builder.Configuration.GetSection("MongoDB"));
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });
        //builder.Services.AddSingleton<IOrderService, OrderService>();
        builder.Services.AddScoped<OrderService>(); 

        builder.Services.AddHostedService<OrderPaymentScheduler>();


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