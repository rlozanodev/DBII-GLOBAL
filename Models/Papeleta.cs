namespace DBII_GLOBAL.Models;

public class Papeleta
{
    public string IdPapeleta { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string RegistroAlumno { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public string Asignatura { get; set; } = string.Empty;
    public string Profesor { get; set; } = string.Empty;
    public string EstadoFinal { get; set; } = string.Empty; // Agendada, Activa, Completada
    
    // Guardamos los materiales como un string JSON plano para simplificar
    public string MaterialesJson { get; set; } = "[]"; 
}