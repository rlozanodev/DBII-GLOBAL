using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;

namespace DBII_GLOBAL.Pages.Personal;

[Authorize(Roles = "Almacén")]
public class InventarioAlmacenModel : PageModel
{
    private readonly DatabaseService _db;
    public InventarioAlmacenModel(DatabaseService db) => _db = db;

    public List<DBII_GLOBAL.Models.Inventario> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _db.MongoDb.GetCollection<DBII_GLOBAL.Models.Inventario>("Inventario")
            .Find(_ => true)
            .ToListAsync();
    }
}