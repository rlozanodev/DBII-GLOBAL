using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using Cassandra;

namespace DBII_GLOBAL.Pages.Administrator;

[Authorize(Roles = "Administrador")]
public class AdminDashboardModel : PageModel
{
    private readonly DatabaseService _db;
    public AdminDashboardModel(DatabaseService db) => _db = db;

    // Estadísticas para los KPI
    public long TotalMateriales { get; set; }
    public int PapeletasActivasRedis { get; set; }
    public long TotalHistoricoCassandra { get; set; }
    public List<Inventario> MaterialesCriticos { get; set; } = new();

    public async Task OnGetAsync()
    {
        // 1. MongoDB: Conteo y Materiales con poco stock (< 5 unidades)
        TotalMateriales = await _db.MongoDb.GetCollection<Inventario>("Inventario").CountDocumentsAsync(_ => true);
        MaterialesCriticos = await _db.MongoDb.GetCollection<Inventario>("Inventario")
            .Find(m => m.StockActual < 5)
            .Limit(5)
            .ToListAsync();

        // 2. Redis: Escaneo de llaves activas hoy
        var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");
        var servidor = _db.RedisConnection.GetServer(_db.RedisConnection.GetEndPoints()[0]);
        PapeletasActivasRedis = servidor.Keys(pattern: $"papeleta:{fechaHoy}:*").Count();

        // 3. Cassandra: Conteo histórico total
        var rs = _db.CassandraSession.Execute("SELECT COUNT(*) FROM historial_papeletas");
        TotalHistoricoCassandra = rs.First().GetValue<long>(0);
    }
}