using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;

namespace DBII_GLOBAL.Pages.Administrator;

[Authorize(Roles = "Administrador")]
public class UserManagementModel : PageModel
{
    private readonly DatabaseService _db;
    public UserManagementModel(DatabaseService db) => _db = db;

    public List<Usuario> Usuarios { get; set; } = new();

    [BindProperty]
    public Usuario NuevoUsuario { get; set; } = new();

    public async Task OnGetAsync()
    {
        Usuarios = await _db.MongoDb.GetCollection<Usuario>("Usuarios")
            .Find(_ => true)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        await _db.MongoDb.GetCollection<Usuario>("Usuarios").InsertOneAsync(NuevoUsuario);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        var filter = Builders<Usuario>.Filter.Eq(u => u.Id, NuevoUsuario.Id);
        var update = Builders<Usuario>.Update
            .Set(u => u.Nombre, NuevoUsuario.Nombre)
            .Set(u => u.Registro, NuevoUsuario.Registro)
            .Set(u => u.Rol, NuevoUsuario.Rol);
            
        // Solo actualizamos password si se proporcionó uno nuevo
        if(!string.IsNullOrEmpty(NuevoUsuario.Password))
            update = update.Set(u => u.Password, NuevoUsuario.Password);

        await _db.MongoDb.GetCollection<Usuario>("Usuarios").UpdateOneAsync(filter, update);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        await _db.MongoDb.GetCollection<Usuario>("Usuarios").DeleteOneAsync(u => u.Id == id);
        return RedirectToPage();
    }
}