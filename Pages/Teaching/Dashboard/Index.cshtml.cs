using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Cassandra;
using MongoDB.Driver; // Necesario para buscar al profesor

namespace DBII_GLOBAL.Pages.Teaching;

[Authorize(Roles = "Profesor")]
public class TeachingDashboardModel : PageModel
{
    private readonly DatabaseService _db;
    public TeachingDashboardModel(DatabaseService db) => _db = db;

    public List<HistorialPapeleta> PapeletasAlumnos { get; set; } = new();

    // Cambiamos a asíncrono para poder consultar MongoDB
    public async Task OnGetAsync()
    {
        // 1. Identificamos al usuario activo
        var registroProfesor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var nombreProfesor = User.Identity?.Name;

        // 2. Buscamos el perfil del profesor en MongoDB para ver sus materias
        var profesorDb = await _db.MongoDb.GetCollection<Usuario>("Usuarios")
            .Find(u => u.Registro == registroProfesor)
            .FirstOrDefaultAsync();

        // Si no tiene asignaturas, creamos una lista vacía para que no truene
        List<string> misAsignaturas = profesorDb?.Asignaturas ?? new List<string>();

        // 3. Consultamos el histórico completo de Cassandra
        // Hacemos un SELECT general porque Cassandra no soporta OR. Filtraremos en C#.
        var cql = "SELECT id_papeleta, registro_alumno, nombre_alumno, asignatura, fecha, estado_final, profesor, materiales_json FROM historial_papeletas";
        var rs = await _db.CassandraSession.ExecuteAsync(new SimpleStatement(cql));

        // 4. Filtro Lógico Híbrido en Memoria
        foreach (var row in rs)
        {
            var pProfesor = row.GetValue<string>("profesor");
            var pAsignatura = row.GetValue<string>("asignatura");

            // CONDICIÓN: 
            // Que la asignatura de la papeleta coincida con las que da el profe
            // O que el nombre/registro del profesor en la papeleta sea el suyo
            if (misAsignaturas.Contains(pAsignatura) || pProfesor == nombreProfesor || pProfesor == registroProfesor)
            {
                PapeletasAlumnos.Add(new HistorialPapeleta
                {
                    IdPapeleta = row.GetValue<string>("id_papeleta"),
                    RegistroAlumno = row.GetValue<string>("registro_alumno"),
                    NombreAlumno = row.GetValue<string>("nombre_alumno"),
                    Asignatura = pAsignatura,
                    Fecha = row.GetValue<string>("fecha"),
                    EstadoFinal = row.GetValue<string>("estado_final"),
                    Profesor = pProfesor,
                    MaterialesJson = row.GetValue<string>("materiales_json")
                });
            }
        }
    }
}