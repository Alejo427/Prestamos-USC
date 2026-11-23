using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrestamosUSC.Core.Data;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Services;

namespace PrestamosUSC.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var conexion = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(conexion))
        {
            MessageBox.Show("Configura ConnectionStrings__DefaultConnection con la ruta absoluta de la base SQLite compartida.", "Préstamos USC");
            return;
        }

        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddDbContext<AppDbContext>(opciones => opciones.UseSqlite(conexion));
        servicios.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        servicios.AddServiciosPrestamos();

        using var proveedor = servicios.BuildServiceProvider();
        Application.Run(new Form1(proveedor.GetRequiredService<IServiceScopeFactory>()));
    }
}
