using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBII_GLOBAL.Models;

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Registro { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public List<string> Asignaturas { get; set; } = new();
}