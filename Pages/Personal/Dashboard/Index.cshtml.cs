using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;
using System.Text.Json;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Cassandra;
using MongoDB.Driver;

namespace DBII_GLOBAL.Pages.Personal;

[Authorize(Roles = "Almacén")]
public class AlmacenDashboardModel : PageModel
{
    private readonly DatabaseService _db;
    public AlmacenDashboardModel(DatabaseService db) => _db = db;

    public List<Papeleta> PapeletasPendientes { get; set; } = new();

    public async Task OnGetAsync()
    {
        var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");
        var servidor = _db.RedisConnection.GetServer(_db.RedisConnection.GetEndPoints()[0]);
        var llaves = servidor.Keys(pattern: $"papeleta:{fechaHoy}:*");

        foreach (var llave in llaves)
        {
            var json = await _db.RedisDb.StringGetAsync(llave);
            if (json.HasValue)
            {
                var p = JsonSerializer.Deserialize<Papeleta>(json.ToString());
                if (p != null && p.Estado != "Finalizada")
                {
                    PapeletasPendientes.Add(p);
                }
            }
        }
    }

    // Acción para marcar como material entregado al alumno (Sigue en Redis)
    public async Task<IActionResult> OnPostEntregarAsync(string id, string fecha)
{
    string llave = $"papeleta:{fecha}:{id}";
    var json = await _db.RedisDb.StringGetAsync(llave);
    
    if (json.HasValue)
    {
        var p = JsonSerializer.Deserialize<Papeleta>(json.ToString());
        // Agregamos la verificación de nulidad para eliminar el warning CS8602
        if (p != null) 
        {
            p.Estado = "Recogida";
            await _db.RedisDb.StringSetAsync(llave, JsonSerializer.Serialize(p), TimeSpan.FromHours(24));
        }
    }
    return RedirectToPage();
}

// Acción para finalizar y persistir en Cassandra
public async Task<IActionResult> OnPostFinalizarAsync(string id, string fecha)
{
    string llave = $"papeleta:{fecha}:{id}";
    var json = await _db.RedisDb.StringGetAsync(llave);

    if (json.HasValue)
    {
        var p = JsonSerializer.Deserialize<Papeleta>(json.ToString());
        if (p != null)
        {
            // --- 1. DEVOLVER STOCK A MONGODB ---
            // Usamos el nombre completo de la clase para evitar el error CS0118
            var inventarioCollection = _db.MongoDb.GetCollection<DBII_GLOBAL.Models.Inventario>("Inventario");
            
            // Corregimos filtros usando la clase explícita
            var filter = Builders<DBII_GLOBAL.Models.Inventario>.Filter.In(i => i.Nombre, p.Materiales) & 
                         Builders<DBII_GLOBAL.Models.Inventario>.Filter.Eq(i => i.Tipo, "Instrumento");
            
            var instrumentosARetornar = await inventarioCollection.Find(filter).ToListAsync();

            foreach (var instrumento in instrumentosARetornar)
            {
                // Incrementamos el stock en 1 para cada instrumento devuelto
                var update = Builders<DBII_GLOBAL.Models.Inventario>.Update.Inc(i => i.StockActual, 1);
                await inventarioCollection.UpdateOneAsync(i => i.Id == instrumento.Id, update);
            }

            // --- 2. PERSISTIR EN CASSANDRA ---
            var cql = @"INSERT INTO historial_papeletas 
                        (id_papeleta, registro_alumno, nombre_alumno, asignatura, fecha, estado_final, profesor, materiales_json) 
                        VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            
            var prepared = await _db.CassandraSession.PrepareAsync(cql);
            string materialesJson = JsonSerializer.Serialize(p.Materiales);

            await _db.CassandraSession.ExecuteAsync(prepared.Bind(
                p.IdPapeleta, 
                p.RegistroAlumno, 
                p.NombreAlumno, 
                p.Asignatura, 
                p.Fecha, 
                "Finalizada", 
                "Pendiente", 
                materialesJson
            ));

            // --- 3. BORRAR DE REDIS ---
            await _db.RedisDb.KeyDeleteAsync(llave);
        }
    }
    return RedirectToPage();
}
}