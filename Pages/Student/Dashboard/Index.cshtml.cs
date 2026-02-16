using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using DBII_GLOBAL.Services;
using StackExchange.Redis;
using DBII_GLOBAL.Models;

namespace DBII_GLOBAL.Pages.Student;

[Authorize(Roles = "Alumno")]
public class StudentDashboardModel : PageModel
{
    private readonly DatabaseService _db;
    public StudentDashboardModel(DatabaseService db) => _db = db;

    // Lista tipada (Seguridad total)
    public List<Papeleta> PapeletasActivas { get; set; } = new();

    public async Task OnGetAsync()
    {
        var registro = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");
        
        var servidor = _db.RedisConnection.GetServer(_db.RedisConnection.GetEndPoints()[0]);
        var llaves = servidor.Keys(pattern: $"papeleta:{fechaHoy}:*");

        foreach (var llave in llaves)
        {
            var json = await _db.RedisDb.StringGetAsync(llave);
            if (json.HasValue)
            {
                // Deserialización directa al modelo
                var p = JsonSerializer.Deserialize<Papeleta>(json.ToString());
                if (p != null && p.RegistroAlumno == registro)
                {
                    PapeletasActivas.Add(p);
                }
            }
        }
    }
}