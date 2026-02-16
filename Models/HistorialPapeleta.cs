using System.Text.Json;

namespace DBII_GLOBAL.Models;

public class HistorialPapeleta
{
    public string IdPapeleta { get; set; } = string.Empty;
    public string RegistroAlumno { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public string Asignatura { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string EstadoFinal { get; set; } = string.Empty;
    public string Profesor { get; set; } = string.Empty;
    public string MaterialesJson { get; set; } = string.Empty;

    // Propiedad calculada para la vista
    public List<string> MaterialesList => 
        string.IsNullOrEmpty(MaterialesJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(MaterialesJson) ?? new List<string>();
}