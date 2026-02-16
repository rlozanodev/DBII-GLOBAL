using MongoDB.Driver;
using StackExchange.Redis;
using Cassandra;

namespace DBII_GLOBAL.Services;

public class DatabaseService
{
    // Propiedades públicas para acceder a los 3 motores
    public IMongoDatabase MongoDb { get; }
    public IDatabase RedisDb { get; }
    public Cassandra.ISession CassandraSession { get; }

    public DatabaseService(IConfiguration config)
    {
        // 1. Conectar MongoDB
        var mongoClient = new MongoClient(config.GetConnectionString("MongoDb"));
        MongoDb = mongoClient.GetDatabase("lab_quimica_db");

        // 2. Conectar Redis
        var redisConn = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!);
        RedisDb = redisConn.GetDatabase();

        // 3. Conectar Cassandra
        var cassandraCluster = Cluster.Builder()
            .AddContactPoint(config.GetConnectionString("Cassandra"))
            .Build();
        CassandraSession = cassandraCluster.Connect("lab_keyspace");
    }
}