using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Data;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Base de datos (Entity Framework Core + SQLite) ----------
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? "Data Source=prestamos.db";
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlite(cadenaConexion, b => b.MigrationsAssembly("PrestamosUSC.Core")));

// ---------- Autenticación y roles (ASP.NET Core Identity) ----------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opciones =>
    {
        opciones.User.RequireUniqueEmail = true;
        opciones.Password.RequiredLength = 6;
        opciones.Password.RequireDigit = true;
        opciones.Password.RequireUppercase = true;
        opciones.Password.RequireLowercase = true;
        opciones.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Account/Login";
    opciones.AccessDeniedPath = "/Account/AccessDenied";
});

// ---------- Servicios de la biblioteca de clases (lógica de negocio) ----------
builder.Services.AddServiciosPrestamos();

// ---------- MVC ----------
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Crea la base, los roles, los usuarios de prueba y los datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    await DbInicializador.InicializarAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
