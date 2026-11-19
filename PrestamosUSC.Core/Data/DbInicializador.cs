using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Enums;

namespace PrestamosUSC.Core.Data;

/// <summary>Crea la base de datos, los roles, usuarios de prueba y datos de ejemplo.</summary>
public static class DbInicializador
{
    public static async Task InicializarAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();

        // Si existen migraciones (dotnet ef migrations add) se aplican; si no, se crea el esquema directamente.
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { RolesApp.Administrador, RolesApp.Estudiante })
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        await CrearUsuarioAsync(userManager, "admin@usc.edu.co", "Administrador USC", "Admin123*", RolesApp.Administrador);
        await CrearUsuarioAsync(userManager, "estudiante@usc.edu.co", "Estudiante de Prueba", "Estudiante123*", RolesApp.Estudiante);

        if (!await db.Recursos.AnyAsync())
        {
            db.Recursos.AddRange(
                new Equipo { Codigo = "EQ-001", Nombre = "Portátil Dell Latitude", Marca = "Dell", Serial = "DL-0001", Ubicacion = "Bodega de sistemas", Descripcion = "Portátil i5, 16 GB RAM" },
                new Equipo { Codigo = "EQ-002", Nombre = "Video beam Epson", Marca = "Epson", Serial = "EP-0002", Ubicacion = "Bodega de sistemas", Descripcion = "Proyector 3500 lúmenes con cable HDMI" },
                new Equipo { Codigo = "EQ-003", Nombre = "Cámara Canon EOS", Marca = "Canon", Serial = "CN-0003", Ubicacion = "Audiovisuales", Descripcion = "Cámara réflex con trípode" },
                new Equipo { Codigo = "EQ-004", Nombre = "Tablet Samsung", Marca = "Samsung", Serial = "SM-0004", Ubicacion = "Biblioteca", Estado = EstadoRecurso.EnMantenimiento },
                new Sala { Codigo = "SA-001", Nombre = "Sala de estudio 1", Capacidad = 8, Piso = 2, Ubicacion = "Biblioteca, piso 2", Descripcion = "Tablero y TV" },
                new Sala { Codigo = "SA-002", Nombre = "Sala de cómputo A", Capacidad = 30, Piso = 1, Ubicacion = "Bloque de ingenierías", Descripcion = "30 computadores" },
                new Sala { Codigo = "SA-003", Nombre = "Auditorio", Capacidad = 100, Piso = 1, Ubicacion = "Bloque central", Descripcion = "Sonido y proyección" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task CrearUsuarioAsync(UserManager<ApplicationUser> um, string email, string nombre, string clave, string rol)
    {
        if (await um.FindByEmailAsync(email) != null) return;

        var usuario = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = nombre
        };
        var resultado = await um.CreateAsync(usuario, clave);
        if (resultado.Succeeded)
            await um.AddToRoleAsync(usuario, rol);
    }
}
