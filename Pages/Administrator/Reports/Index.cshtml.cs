namespace DBII_GLOBAL.Pages.Administrator.Reports;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador")] // Solo el admin entra aquí
public class AdminReportsModel : PageModel
{
    public void OnGet()
    {
    }
}
