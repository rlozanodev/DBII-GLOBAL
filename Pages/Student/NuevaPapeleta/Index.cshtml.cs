namespace DBII_GLOBAL.Pages.Student; 

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Cassandra;

[Authorize(Roles = "Alumno")]
public class NuevaPapeletaModel : PageModel
{
    private readonly DatabaseService _db;
    public NuevaPapeletaModel(DatabaseService db) => _db = db;

    public List<Inventario> MaterialesDisponibles { get; set; } = new();
    public List<Usuario> Profesores { get; set; } = new();
    
    [BindProperty]
    public string Asignatura { get; set; } = string.Empty;

    [BindProperty]
    public string Profesor { get; set; } = string.Empty;

    // Recibe el JSON del carrito
    [BindProperty]
    public string MaterialesJsonSubmit { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        MaterialesDisponibles = await _db.MongoDb.GetCollection<Inventario>("Inventario")
                                       .Find(m => m.StockActual > 0)
                                       .SortBy(m => m.Nombre)
                                       .ToListAsync();

        Profesores = await _db.MongoDb.GetCollection<Usuario>("Usuarios")
                                       .Find(u => u.Rol == "Profesor")
                                       .SortBy(u => u.Nombre)
                                       .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var registro = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var nombre = User.Identity?.Name;

            // 1. Deserializar el JSON del frontend a nuestra clase auxiliar
            var materialesPedidos = JsonSerializer.Deserialize<List<MaterialPedido>>(MaterialesJsonSubmit);
            
            if (materialesPedidos == null || !materialesPedidos.Any())
            {
                ModelState.AddModelError("", "Debes agregar al menos un material a la lista.");
                await OnGetAsync();
                return Page();
            }

            // 2. Formatear a List<string> para respetar TU modelo original
            var materialesFormateados = materialesPedidos.Select(m => $"{m.Cantidad}x {m.Nombre}").ToList();
            
            var papeleta = new Papeleta
            {
                IdPapeleta = Guid.NewGuid().ToString().Substring(0, 8),
                RegistroAlumno = registro ?? string.Empty,
                NombreAlumno = nombre ?? string.Empty,
                Asignatura = Asignatura,
                Profesor = Profesor, // Asignamos el profesor seleccionado
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Estado = "Agendada",
                Materiales = materialesFormateados // Pasamos la lista de strings (Ej: "2x Matraz")
            };

            // 3. Pre-guardar en Cassandra (Usando papeleta.Profesor)
            var cql = "INSERT INTO historial_papeletas (id_papeleta, registro_alumno, nombre_alumno, asignatura, fecha, estado_final, profesor, materiales_json) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            var prepared = await _db.CassandraSession.PrepareAsync(cql);
            await _db.CassandraSession.ExecuteAsync(prepared.Bind(
                papeleta.IdPapeleta,
                papeleta.RegistroAlumno,
                papeleta.NombreAlumno,
                papeleta.Asignatura,
                papeleta.Fecha,
                "Agendada",
                papeleta.Profesor, 
                JsonSerializer.Serialize(papeleta.Materiales)
            ));

            // 4. Guardar en Redis
            string jsonPapeleta = JsonSerializer.Serialize(papeleta);
            string llaveRedis = $"papeleta:{papeleta.Fecha}:{papeleta.IdPapeleta}";
            await _db.RedisDb.StringSetAsync(llaveRedis, jsonPapeleta, TimeSpan.FromHours(24));

            // 5. Restar stock real en MongoDB basado en las cantidades solicitadas
            var inventarioCollection = _db.MongoDb.GetCollection<Inventario>("Inventario");
            foreach (var item in materialesPedidos)
            {
                var update = Builders<Inventario>.Update.Inc(i => i.StockActual, -item.Cantidad);
                await inventarioCollection.UpdateOneAsync(i => i.Id == item.Id, update);
            }

            return RedirectToPage("/Student/Dashboard/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al procesar la papeleta: " + ex.Message);
            await OnGetAsync();
            return Page();
        }
    }
}