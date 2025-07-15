using Microsoft.OpenApi.Models;
using NoSQL.WebApi.Repositories;
using NoSQL.WebApi.Services;
using System.Reflection;

namespace NoSQL.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Messaging NoSQL API",
                Version = "v1"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddSingleton<CassandraClient>();
        builder.Services.AddSingleton(sp => sp.GetRequiredService<CassandraClient>().GetSession());

        builder.Services.AddSingleton<ICassandraMigrationService, CassandraMigrationService>();
        builder.Services.AddScoped<IMessageRepository, CassandraMessageRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        using (var scope = app.Services.CreateScope())
        {
            var migrationService = scope.ServiceProvider.GetRequiredService<ICassandraMigrationService>();
            await migrationService.InitializeDatabaseSchemaAsync();

            var cassandraClient = scope.ServiceProvider.GetRequiredService<CassandraClient>();
            cassandraClient.ChangeKeyspace("messaging_app");
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}