namespace NoSQL.WebApi.Services;

public interface ICassandraMigrationService
{
    Task InitializeDatabaseSchemaAsync();
}