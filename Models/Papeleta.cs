namespace DBII_GLOBAL.Models;

public class Papeleta
{
    public string IdPapeleta { get; set; } = string.Empty;
    public string RegistroAlumno { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public string Asignatura { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Materiales { get; set; } = new();
    public string Profesor { get; set; } = string.Empty;
    
}
public class MaterialPedido
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}