using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MongoDB.Driver;
using DBII_GLOBAL.Services;
using DBII_GLOBAL.Models;
using Microsoft.AspNetCore.Authorization;

namespace DBII_GLOBAL.Pages.Login;

[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly DatabaseService _db;
    public LoginModel(DatabaseService db) => _db = db;

    [BindProperty] public string Registro { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string? DebugInfo { get; set; } // Para ver errores en la UI

    public async Task<IActionResult> OnPostAsync()
    {
        try 
        {
            // LOG INICIAL: Ver qué llega al servidor
            Console.WriteLine($"\n--- [DEBUG LOGIN] ---");
            Console.WriteLine($"Registro: '{Registro}'");
            Console.WriteLine($"Password: '{Password}'");

            // Validar que no vengan vacíos (KISS)
            if (string.IsNullOrEmpty(Registro) || string.IsNullOrEmpty(Password))
            {
                Mensaje = "El registro y la contraseña son obligatorios.";
                return Page();
            }

            var coleccion = _db.MongoDb.GetCollection<Usuario>("Usuarios");
            
            // Intento de consulta a MongoDB
            var usuario = await coleccion.Find(u => 
                u.Registro.Trim() == Registro.Trim() && 
                u.Password.Trim() == Password.Trim())
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                Console.WriteLine("[DEBUG] Usuario no encontrado en la DB.");
                Mensaje = "Credenciales incorrectas.";
                return Page();
            }

            Console.WriteLine($"[DEBUG] Usuario hallado: {usuario.Nombre} con Rol: {usuario.Rol}");

            // Configuración de Identidad
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, (string)usuario.Nombre),
                new Claim(ClaimTypes.NameIdentifier, (string)usuario.Registro),
                new Claim(ClaimTypes.Role, (string)usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // Redirección
            return usuario.Rol switch
            {
                "Alumno" => RedirectToPage("/Student/Dashboard/Index"),
                "Almacén" => RedirectToPage("/Personal/Dashboard/Index"),
                "Profesor" => RedirectToPage("/Teaching/Dashboard/Index"),
                "Administrador" => RedirectToPage("/Administrator/Reports/Index"),
                _ => RedirectToPage("/Index")
            };
        }
        catch (MongoException ex)
        {
            // Error específico de base de datos
            Console.WriteLine($"[ERROR MONGODB]: {ex.Message}");
            Mensaje = "Error de conexión con la base de datos.";
            DebugInfo = ex.ToString();
            return Page();
        }
        catch (Exception ex)
        {
            // Error general
            Console.WriteLine($"[ERROR GENERAL]: {ex.Message}");
            Mensaje = "Ocurrió un error inesperado.";
            DebugInfo = ex.ToString();
            return Page();
        }
    }
}