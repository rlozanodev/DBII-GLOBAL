using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBII_GLOBAL.Models;

// En Models/Usuario.cs
public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    [BsonElement("registro")] // <--- DEBE coincidir exactamente con TablePlus
    public string Registro { get; set; } = "";

    [BsonElement("password")] // <--- DEBE coincidir exactamente con TablePlus
    public string Password { get; set; } = "";

    [BsonElement("nombre")]
    public string Nombre { get; set; } = "";

    [BsonElement("rol")]
    public string Rol { get; set; } = "";

    [BsonElement("asignaturas")] // Mapeo del Array de strings
    public List<string> Asignaturas { get; set; } = new List<string>();
}