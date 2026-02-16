using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBII_GLOBAL.Models;

public class Inventario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [BsonElement("stock_actual")]
    public int StockActual { get; set; }
}