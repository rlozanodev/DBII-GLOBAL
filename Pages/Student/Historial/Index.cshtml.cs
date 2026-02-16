using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Cassandra;

namespace DBII_GLOBAL.Pages.Student;

[Authorize(Roles = "Alumno")]
public class HistorialModel : PageModel
{
    private readonly DatabaseService _db;
    public HistorialModel(DatabaseService db) => _db = db;

    public List<HistorialPapeleta> Historial { get; set; } = new();

    public void OnGet()
    {
        var registro = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Consulta corregida con tus campos exactos
        var cql = "SELECT id_papeleta, registro_alumno, nombre_alumno, asignatura, fecha, estado_final, profesor, materiales_json " +
                  "FROM historial_papeletas WHERE registro_alumno = ? ALLOW FILTERING";
        
        var prepared = _db.CassandraSession.Prepare(cql);
        var rs = _db.CassandraSession.Execute(prepared.Bind(registro));

        foreach (var row in rs)
        {
            Historial.Add(new HistorialPapeleta
            {
                IdPapeleta = row.GetValue<string>("id_papeleta"),
                RegistroAlumno = row.GetValue<string>("registro_alumno"),
                NombreAlumno = row.GetValue<string>("nombre_alumno"),
                Asignatura = row.GetValue<string>("asignatura"),
                Fecha = row.GetValue<string>("fecha"),
                EstadoFinal = row.GetValue<string>("estado_final"),
                Profesor = row.GetValue<string>("profesor"),
                MaterialesJson = row.GetValue<string>("materiales_json")
            });
        }
    }
}