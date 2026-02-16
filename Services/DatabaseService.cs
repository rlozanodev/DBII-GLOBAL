using MongoDB.Driver;
using StackExchange.Redis;
using Cassandra;

namespace DBII_GLOBAL.Services;

public class DatabaseService
{
    public IMongoDatabase MongoDb { get; }
    public IConnectionMultiplexer RedisConnection { get; }
    public IDatabase RedisDb { get; } // <--- ESTA LÍNEA FALTABA
    public Cassandra.ISession CassandraSession { get; }

    public DatabaseService(IConfiguration config)
    {
        // 1. Conectar MongoDB
        var mongoClient = new MongoClient(config.GetConnectionString("MongoDb"));
        MongoDb = mongoClient.GetDatabase("lab_quimica_db");

        // 2. Conectar Redis
        var redisConnString = config.GetConnectionString("Redis");
        RedisConnection = ConnectionMultiplexer.Connect(redisConnString!);
        RedisDb = RedisConnection.GetDatabase(); // Ahora el compilador ya sabe dónde guardarlo

        // 3. Conectar Cassandra
        var cassandraCluster = Cluster.Builder()
            .AddContactPoint(config.GetConnectionString("Cassandra"))
            .Build();
        CassandraSession = cassandraCluster.Connect("lab_keyspace");
    }
}