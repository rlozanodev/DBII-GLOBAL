using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Cassandra;

namespace DBII_GLOBAL.Pages.Administrator;

[Authorize(Roles = "Administrador")]
public class GlobalHistoryModel : PageModel
{
    private readonly DatabaseService _db;
    public GlobalHistoryModel(DatabaseService db) => _db = db;

    public List<HistorialPapeleta> TodosLosRegistros { get; set; } = new();

    public void OnGet()
    {
        // Consulta sin filtros para el Administrador
        var cql = "SELECT id_papeleta, registro_alumno, nombre_alumno, asignatura, fecha, estado_final, profesor, materiales_json FROM historial_papeletas";
        
        var rs = _db.CassandraSession.Execute(cql);

        foreach (var row in rs)
        {
            TodosLosRegistros.Add(new HistorialPapeleta
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