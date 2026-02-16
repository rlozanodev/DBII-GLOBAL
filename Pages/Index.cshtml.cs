using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBII_GLOBAL.Pages;

[Authorize] // <--- Esto obliga a loguearse para ver el Index
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return rol switch
        {
            "Alumno" => RedirectToPage("/Student/Dashboard/Index"),
            "Almacén" => RedirectToPage("/Personal/Dashboard/Index"),
            "Profesor" => RedirectToPage("/Teaching/Dashboard/Index"),
            "Administrador" => RedirectToPage("/Administrator/Reports/Index"),
            _ => RedirectToPage("/Login/Login") // Si algo falla, al login
        };
    }
}
