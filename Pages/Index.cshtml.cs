using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBII_GLOBAL.Pages;

[Authorize] // <--- Esto obliga a loguearse para ver el Index
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // Si llega aquí es porque ya está logueado, redirigimos por rol
        var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return rol switch
        {
            "Alumno" => RedirectToPage("/Student/Dashboard"),
            "Almacén" => RedirectToPage("/Personal/Dashboard"),
            "Profesor" => RedirectToPage("/Teaching/Dashboard"),
            "Administrador" => RedirectToPage("/Administrator/Reportes"),
            _ => Page()
        };
    }
}
