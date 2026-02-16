using DBII_GLOBAL.Services; // Asegúrate de agregar esto arriba
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // 1. Obligar a estar autenticado en TODAS las carpetas
    options.Conventions.AuthorizeFolder("/Student");
    options.Conventions.AuthorizeFolder("/Teaching");
    options.Conventions.AuthorizeFolder("/Personal");
    options.Conventions.AuthorizeFolder("/Administrator");

    // 2. Permitir acceso anónimo a la carpeta de Login
    options.Conventions.AllowAnonymousToFolder("/Login");
});

// INYECTAR NUESTRO SERVICIO DE BASES DE DATOS COMO SINGLETON
builder.Services.AddSingleton<DatabaseService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 3. ¡IMPORTANTE! Actualiza la ruta del Login aquí
        options.LoginPath = "/Login/Login"; 
        options.AccessDeniedPath = "/Index";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();