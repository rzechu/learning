using Cassandra;

namespace NoSQL.WebApi.Services;

public class CassandraClient : IDisposable
{
    private readonly ICluster _cluster;
    private readonly Cassandra.ISession? _session;
    private readonly ILogger<CassandraClient> _logger;
    private readonly List<string> _contactPoints;
    private readonly int _port;
    private readonly string _dc;

    public CassandraClient(IConfiguration configuration, ILogger<CassandraClient> logger)
    {
        _logger = logger;
        var cassandraSettings = configuration.GetSection("CassandraSettings");
        _contactPoints = cassandraSettings.GetSection("ContactPoints").Get<List<string>>() ?? new List<string> { "cassandra1" };
        _port = cassandraSettings.GetValue<int>("Port", 9042);
        _dc = cassandraSettings.GetValue<string>("DataCenter") ?? "datacenter1";

        _cluster = Cluster.Builder()
                        .AddContactPoints(_contactPoints.ToArray())
                        .WithPort(_port)
                        .WithLoadBalancingPolicy(new DCAwareRoundRobinPolicy(_dc))
                        .Build();

        _session = _cluster.Connect();
        _logger.LogTrace("Connected to Cassandra (no keyspace).");
    }

    public Cassandra.ISession GetSession() => _session!;

    public void ChangeKeyspace(string keyspace)
    {
        _session?.ChangeKeyspace(keyspace);
        _logger.LogTrace($"Changed session to keyspace: {keyspace}");
    }

    public void Dispose()
    {
        _session?.Dispose();
        _cluster?.Dispose();
        _logger.LogTrace("Disconnected from Cassandra.");
    }
}