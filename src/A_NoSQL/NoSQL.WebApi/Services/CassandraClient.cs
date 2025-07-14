using Cassandra;

namespace NoSQL.WebApi.Services;

public class CassandraClient : IDisposable
{
    private readonly Cassandra.ISession _session;
    private readonly ICluster _cluster;
    private readonly ILogger<CassandraClient> _logger;

    public CassandraClient(IConfiguration configuration, ILogger<CassandraClient> logger)
    {
        _logger = logger;
        var cassandraSettings = configuration.GetSection("CassandraSettings");
        var contactPoints = cassandraSettings.GetSection("ContactPoints").Get<List<string>>();
        var port = cassandraSettings.GetValue<int>("Port");
        var keyspace = cassandraSettings.GetValue<string>("Keyspace");

        _cluster = Cluster.Builder()
                        .AddContactPoints(contactPoints.ToArray())
                        .WithPort(port)
                        .WithLoadBalancingPolicy(new DCAwareRoundRobinPolicy("datacenter1"))
                        .Build();

        _session = _cluster.Connect(keyspace);
        _logger.LogTrace($"Successfully connected to Cassandra keyspace: {keyspace}");
    }

    public Cassandra.ISession GetSession()
        => _session;

    public void Dispose()
    {
        _session?.Dispose();
        _cluster?.Dispose();
        _logger.LogTrace("Disconnected from Cassandra.");
    }
}