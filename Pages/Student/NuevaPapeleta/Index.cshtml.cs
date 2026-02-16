namespace DBII_GLOBAL.Pages.Student; // Unificado para todo lo que haga el alumno

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;

[Authorize(Roles = "Alumno")]
public class NuevaPapeletaModel : PageModel
{
    private readonly DatabaseService _db;
    public NuevaPapeletaModel(DatabaseService db) => _db = db;

    public List<Inventario> MaterialesDisponibles { get; set; } = new();
    
    [BindProperty]
    public List<string> MaterialesSeleccionados { get; set; } = new();
    
    [BindProperty]
    public string Asignatura { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        // Carga desde MongoDB
        MaterialesDisponibles = await _db.MongoDb.GetCollection<Inventario>("Inventario")
                                       .Find(m => m.StockActual > 0)
                                       .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var registro = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var nombre = User.Identity?.Name;
            
            var papeleta = new
            {
                IdPapeleta = Guid.NewGuid().ToString().Substring(0, 8),
                RegistroAlumno = registro,
                NombreAlumno = nombre,
                Asignatura = Asignatura,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Estado = "Agendada",
                Materiales = MaterialesSeleccionados
            };

            // Guardar en Redis
            string jsonPapeleta = JsonSerializer.Serialize(papeleta);
            string llaveRedis = $"papeleta:{papeleta.Fecha}:{papeleta.IdPapeleta}";
            
            await _db.RedisDb.StringSetAsync(llaveRedis, jsonPapeleta, TimeSpan.FromHours(24));

            return RedirectToPage("/Student/Dashboard/Index");
        }
        catch (Exception)
        {
            return Page();
        }
    }
}