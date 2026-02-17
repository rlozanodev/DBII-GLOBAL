using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;

namespace DBII_GLOBAL.Pages.Administrator;

[Authorize(Roles = "Administrador")]
public class InventoryManagementModel : PageModel
{
    private readonly DatabaseService _db;
    public InventoryManagementModel(DatabaseService db) => _db = db;

    public List<Inventario> Catalogo { get; set; } = new();

    [BindProperty]
    public Inventario NuevoItem { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Carga de todo el inventario desde MongoDB
        Catalogo = await _db.MongoDb.GetCollection<Inventario>("Inventario")
            .Find(_ => true)
            .SortBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _db.MongoDb.GetCollection<Inventario>("Inventario").InsertOneAsync(NuevoItem);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        await _db.MongoDb.GetCollection<Inventario>("Inventario").DeleteOneAsync(m => m.Id == id);
        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        // Usamos el ID del objeto que viene del formulario para buscar y actualizar
        var filter = Builders<Inventario>.Filter.Eq(m => m.Id, NuevoItem.Id);
        var update = Builders<Inventario>.Update
            .Set(m => m.Nombre, NuevoItem.Nombre)
            .Set(m => m.Tipo, NuevoItem.Tipo)
            .Set(m => m.StockActual, NuevoItem.StockActual);

        await _db.MongoDb.GetCollection<Inventario>("Inventario").UpdateOneAsync(filter, update);
        return RedirectToPage();
    }
}